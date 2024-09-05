using System.Diagnostics;
using Cardano.Sync.Data;
using Cardano.Sync.Data.Models;
using Cardano.Sync.Reducers;
using Cardano.Sync.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PallasDotnet;
using PallasDotnet.Models;

namespace Cardano.Sync.Workers;

public class CriticalNodeExceptionz(string message) : Exception(message) { }

public class OldCardanoIndexWorker<T>(
    IConfiguration configuration,
    ILogger<CardanoIndexWorker<T>> logger,
    IDbContextFactory<T> dbContextFactory,
    IEnumerable<IReducer> reducers
) : BackgroundService where T : CardanoDbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(reducers.Select(reducer => ChainSyncReducerAsync(reducer, stoppingToken)));
    }

    private async Task ChainSyncReducerAsync(IReducer reducer, CancellationToken stoppingToken)
    {
        using T dbContext = dbContextFactory.CreateDbContext();
        CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        NodeClient nodeClient = new(); // @TODO: Update to the latest version of Pallas.NET

        string reducerName = ArgusUtils.GetTypeNameWithoutGenerics(reducer.GetType());
        ReducerState? reducerState = await ArgusUtils.GetReducerStateAsync(dbContext, reducerName, stoppingToken);

        void Handler(object? sender, ChainSyncNextResponseEventArgs e)
        {
            if (e.NextResponse.Action == NextResponseAction.Await) return;

            Stopwatch stopwatch = new();
            stopwatch.Start();

            NextResponse response = e.NextResponse;

            logger.Log(
                LogLevel.Information, "[{reducer}]: New Chain Event {Action}: {Slot} Block: {Block}",
                reducerName,
                response.Action,
                response.Block.Slot,
                response.Block.Number
            );

            Dictionary<NextResponseAction, Func<IReducer, NextResponse, T, string, CancellationToken, Task>> actionMethodMap = new()
            {
                { NextResponseAction.RollForward, ProcessRollForwardAsync },
                { NextResponseAction.RollBack, ProcessRollBackAsync }
            };

            // Execute reducer action
            Func<IReducer, NextResponse, T, string, CancellationToken, Task> reducerAction = actionMethodMap[response.Action];
            reducerAction(reducer, response, dbContext, reducerName, stoppingToken).Wait(stoppingToken);

            Task.Run(async () =>
            {
                // @TODO: Add a function to save reducer states
                if (reducerState is null)
                {
                    dbContext.ReducerStates.Add(new()
                    {
                        Name = ArgusUtils.GetTypeNameWithoutGenerics(reducer.GetType()),
                        Slot = response.Block.Slot,
                        Hash = response.Block.Hash.ToHex()
                    });
                }
                else
                {
                    reducerState.Slot = response.Block.Slot;
                    reducerState.Hash = response.Block.Hash.ToHex();
                }
                await dbContext.SaveChangesAsync();
            }, stoppingToken).Wait(stoppingToken);

            stopwatch.Stop();

            logger.Log(
                LogLevel.Information,
                "[{reducer}]: Processed Chain Event {Action}: {Slot} Block: {Block} in {ElapsedMilliseconds} ms, Mem: {MemoryUsage} MB",
                ArgusUtils.GetTypeNameWithoutGenerics(reducer.GetType()),
                response.Action,
                response.Block.Slot,
                response.Block.Number,
                stopwatch.ElapsedMilliseconds,
                Math.Round(GetCurrentMemoryUsageInMB(), 2)
            );
        }

        void DisconnectedHandler(object? sender, EventArgs e)
        {
            linkedCts.Cancel();
        }

        nodeClient.ChainSyncNextResponse += Handler;
        nodeClient.Disconnected += DisconnectedHandler;

        ulong startSlot = configuration.GetValue<ulong>($"CardanoIndexStartSlot_{reducerName}");
        string? startHash = configuration.GetValue<string>($"CardanoIndexStartHash_{reducerName}");

        if (startSlot == 0 && startHash is null)
        {
            startSlot = configuration.GetValue<ulong>("CardanoIndexStartSlot");
            startHash = configuration.GetValue<string>("CardanoIndexStartHash");
        }

        if (reducerState is not null)
        {
            startSlot = reducerState.Slot;
            startHash = reducerState.Hash;
        }

        Point tip = await nodeClient.ConnectAsync(configuration.GetValue<string>("CardanoNodeSocketPath")!, configuration.GetValue<ulong>("CardanoNetworkMagic"));
        await nodeClient.StartChainSyncAsync(new(
            startSlot,
            Hash.FromHex(startHash!)
        ));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);
            }
        }
        finally
        {
            nodeClient.ChainSyncNextResponse -= Handler;
            nodeClient.Disconnected -= DisconnectedHandler;
        }
    }

    private async Task ProcessRollForwardAsync(IReducer reducer, NextResponse response, T dbContext, string reducerName, CancellationToken stoppingToken)
    {
        try
        {
            Stopwatch reducerStopwatch = new();
            Type[] reducerDependencies = ReducerDependencyResolver.GetReducerDependencies(reducer.GetType());

            if (reducerDependencies.Any())
            {
                while (true)
                {
                    bool canProceed = true;
                    foreach (Type dependency in reducerDependencies)
                    {
                        string dependencyName = ArgusUtils.GetTypeNameWithoutGenerics(dependency);
                        ReducerState? dependencyState = await ArgusUtils.GetReducerStateAsync(dbContext, dependencyName, stoppingToken);

                        if (dependencyState == null || dependencyState.Slot < response.Block.Slot)
                        {
                            logger.Log(LogLevel.Information, "[{Reducer}]: Waiting for dependency {Dependency} Slot {depdencySlot} < {currentSlot}",
                                reducerName,
                                dependencyName,
                                dependencyState?.Slot,
                                response.Block.Slot
                            );
                            canProceed = false;
                            break;
                        }
                    }
                    if (canProceed) break;
                    await Task.Delay(1000, stoppingToken);
                }
            }

            await reducer.RollForwardAsync(response);
            reducerStopwatch.Stop();
            logger.Log(LogLevel.Information, "Processed RollForwardAsync[{}] in {ElapsedMilliseconds} ms", reducerName, reducerStopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Error in RollForwardAsync");
            Environment.Exit(1);
        }
    }

    private async Task ProcessRollBackAsync(IReducer reducer, NextResponse response, T dbContext, string reducerName, CancellationToken stoppingToken)
    {
        try
        {
            ulong reducerCurrentSlot = dbContext.ReducerStates
                .AsNoTracking()
                .Where(rs => rs.Name == reducerName)
                .Select(rs => rs.Slot)
                .FirstOrDefault();

            if (reducerCurrentSlot > 0)
            {
                ulong maxAdditionalRollbackSlots = 100 * 20;
                ulong requestedRollBackSlot = response.Block.Slot;
                if (reducerCurrentSlot - requestedRollBackSlot > maxAdditionalRollbackSlots)
                {
                    logger.Log(
                        LogLevel.Error,
                        "RollBackwardAsync[{}] Requested RollBack Slot {requestedRollBackSlot} is more than {maxAdditionalRollbackSlots} slots behind current slot {reducerCurrentSlot}.",
                        reducerName,
                        requestedRollBackSlot,
                        maxAdditionalRollbackSlots,
                        reducerCurrentSlot
                    );

                    throw new CriticalNodezException("Rollback, Critical Error, Aborting");
                }
            }

            Stopwatch reducerStopwatch = new();
            reducerStopwatch.Start();
            await reducer.RollBackwardAsync(response);
            reducerStopwatch.Stop();
            logger.Log(LogLevel.Information, "Processed RollBackwardAsync[{}] in {ElapsedMilliseconds} ms", reducerName, reducerStopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Error in RollBackwardAsync");
            Environment.Exit(1);
        }
    }

    public static double GetCurrentMemoryUsageInMB()
    {
        Process currentProcess = Process.GetCurrentProcess();
        long memoryUsed = currentProcess.WorkingSet64;
        double memoryUsedMb = memoryUsed / 1024.0 / 1024.0;

        return memoryUsedMb;
    }
}





