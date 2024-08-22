using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Cardano.Sync.Data;
using Cardano.Sync.Data.Models.Enums;
using Cardano.Sync.Utils;
using TransactionOutputEntity = Cardano.Sync.Data.Models.TransactionOutput;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;

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

        List<TransactionOutputEntity> spentOutputs = await _dbContext.TransactionOutputs
            .Where(o => o.SpentSlot > rollbackSlot)
            .ToListAsync();

        if (spentOutputs.Any())
        {
            foreach (TransactionOutputEntity output in spentOutputs)
            {
                output.SpentSlot = null;
                output.UtxoStatus = UtxoStatus.Unspent;
            }
            _dbContext.TransactionOutputs.UpdateRange(spentOutputs);
        }

        _dbContext.TransactionOutputs.RemoveRange(
            _dbContext.TransactionOutputs.AsNoTracking().Where(o => o.Slot > rollbackSlot && o.SpentSlot == null)
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
        List<(string, string)> inputHashes = block.TransactionBodies
            .SelectMany(txBody => txBody.Inputs.Select(input => (input.Id.ToHex(), input.Index.ToString())))
            .ToList();

        Expression<Func<TransactionOutputEntity, bool>> predicate = PredicateBuilder.False<TransactionOutputEntity>();

        foreach ((string id,string index) in inputHashes)
        {
            predicate = predicate.Or(p => p.Id == id && p.Index.ToString() == index);
        }

        List<TransactionOutputEntity> existingOutputs = await _dbContext.TransactionOutputs
            .Where(predicate)
            .ToListAsync();

        if (existingOutputs.Any())
        {
            existingOutputs.ForEach(eo =>
            {
                eo.SpentSlot = block.Slot;
                eo.UtxoStatus = UtxoStatus.Spent;
            });
            _dbContext.TransactionOutputs.UpdateRange(existingOutputs);
        }
    }

    private void ProcessOutputs(Block block)
    {
        List<TransactionOutputEntity> outputEntities = block.TransactionBodies
            .SelectMany(txBody => txBody.Outputs.Select(output =>
                DataUtils.MapTransactionOutputEntity(txBody.Id.ToHex(), block.Slot, output, UtxoStatus.Unspent)))
            .ToList();

        _dbContext.TransactionOutputs.AddRange(outputEntities);
    }
}