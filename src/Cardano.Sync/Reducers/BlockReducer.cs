using Cardano.Sync.Data;
using Microsoft.EntityFrameworkCore;
using PallasDotnet.Models;
using BlockEntity = Cardano.Sync.Data.Models.Block;
namespace Cardano.Sync.Reducers;


// Model

public class BlockReducer<T>(IDbContextFactory<T> dbContextFactory) : IReducer where T : CardanoDbContext
{
    private T _dbContext = default!;

    public async Task RollBackwardAsync(NextResponse response)
    {
        _dbContext = dbContextFactory.CreateDbContext();
        _dbContext.Blocks.RemoveRange(_dbContext.Blocks.AsNoTracking().Where(b => b.Slot > response.Block.Slot));
        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    public async Task RollForwardAsync(NextResponse response)
    {
        _dbContext = dbContextFactory.CreateDbContext();

        _dbContext.Blocks.Add(new BlockEntity
        {
            Id = response.Block.Hash.ToHex(),
            Number = response.Block.Number,
            Slot = response.Block.Slot,
            BlockCbor = []
        });

        await _dbContext.SaveChangesAsync();
        _dbContext.Dispose();
    }

    // OnModelCreating
}