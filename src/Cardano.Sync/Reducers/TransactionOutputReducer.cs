using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Cardano.Sync.Data;
using Cardano.Sync.Data.Models.Enums;
using TransactionOutputEntity = Cardano.Sync.Data.Models.TransactionOutput;
using Microsoft.Extensions.Configuration;

namespace Cardano.Sync.Reducers;

public class TransactionOutputReducer<T>(
    IDbContextFactory<T> dbContextFactory,
    IConfiguration configuration
) : ICoreReducer where T : CardanoDbContext
{
    private T _dbContext = default!;

    public async Task RollBackwardAsync(NextResponse response)
    {
        _dbContext = dbContextFactory.CreateDbContext();
        ulong rollbackSlot = response.Block.Slot;
        string? schema = configuration.GetConnectionString("CardanoContextSchema");

        _dbContext.TransactionOutputs.RemoveRange(
            _dbContext.TransactionOutputs.AsNoTracking().Where(o => o.Slot > rollbackSlot)
        );

        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task RollForwardAsync(NextResponse response)
    {
        if (response.Block.TransactionBodies.Any())
        {
            _dbContext = dbContextFactory.CreateDbContext();

            await ProcessInputsAsync(response.Block);
            ProcessOutputs(response.Block);

            await _dbContext.SaveChangesAsync();
            _dbContext.Dispose();
        }
    }

    private async Task ProcessInputsAsync(Block block)
    {
        List<string> inputHashes = block.TransactionBodies
            .SelectMany(txBody => txBody.Inputs.Select(input => input.Id.ToHex() + input.Index.ToString()))
            .Distinct()
            .ToList();

        List<TransactionOutputEntity> existingOutputs = await _dbContext.TransactionOutputs
            .AsNoTracking()
            .Where(to => inputHashes.Contains(to.Id + to.Index.ToString()))
            .ToListAsync();

        if (existingOutputs.Any())
        {
            existingOutputs.ForEach(eo =>
            {
                eo.UtxoStatus = UtxoStatus.Spent;
                eo.DateSpent = DateTimeOffset.UtcNow;
            });
            _dbContext.TransactionOutputs.UpdateRange(existingOutputs);
        }
    }

    private void ProcessOutputs(Block block)
    {
        List<TransactionOutputEntity> outputEntities = block.TransactionBodies
            .SelectMany(txBody => txBody.Outputs.Select(output =>
                Utils.MapTransactionOutputEntity(txBody.Id.ToHex(), block.Slot, output, UtxoStatus.Unspent)))
            .ToList();

        _dbContext.TransactionOutputs.AddRange(outputEntities);
    }
}