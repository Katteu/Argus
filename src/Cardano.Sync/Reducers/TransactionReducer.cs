using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using Cardano.Sync.Data;
using Microsoft.Extensions.Configuration;
using Cardano.Sync.Data.Models;

namespace Cardano.Sync.Reducers;

public class TransactionReducer<T>(
    IDbContextFactory<T> dbContextFactory,
    IConfiguration configuration
) : IReducer where T : CardanoDbContext
{
    private T _dbContext = default!;

    public async Task RollBackwardAsync(NextResponse response)
    {
        _dbContext = dbContextFactory.CreateDbContext();
        ulong rollbackSlot = response.Block.Slot;
        string? schema = configuration.GetConnectionString("CardanoContextSchema");

        _dbContext.Transactions.RemoveRange(
            _dbContext.Transactions.AsNoTracking().Where(o => o.Slot > rollbackSlot)
        );

        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task RollForwardAsync(NextResponse response)
    {
        if (response.Block.TransactionBodies.Any())
        {
            _dbContext = dbContextFactory.CreateDbContext();

            Transaction transaction = new()
            {
                Slot = response.Block.Slot,
                Hash = response.Block.Hash.ToHex(),
                TxCbor = []
            };

            _dbContext.Transactions.Add(transaction);

            await _dbContext.SaveChangesAsync();
            _dbContext.Dispose();
        }
    }
}