using Cardano.Sync.Example.Data;
using Cardano.Sync.Reducers;
using PallasDotnet.Models;

namespace Cardano.Sync.Example.Reducers;

// Place Model here
// public record Transaction Model
// {
//     public ulong Slot { get; set; }
//     public string Hash { get; set; } = default!;
//     public byte[] TxCbor { get; set; } = [];
// }

[ReducerDepends(typeof(BlockReducer<>))]
public class TestReducer(
    ILogger<TestReducer> logger
) : IReducer
{
    public async Task RollForwardAsync(NextResponse response)
    {
        logger.LogInformation("Rolling forward {slot}", response.Block.Slot);
        await Task.CompletedTask;
    }

    public async Task RollBackwardAsync(NextResponse response)
    {
        logger.LogInformation("Rolling backward {slot}", response.Block.Slot);
        await Task.CompletedTask;
    }

    // OnModelCreating
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     modelBuilder.HasDefaultSchema(_configuration.GetConnectionString("CardanoContextSchema"));
    //     modelBuilder.Entity<Block>().HasIndex(b => b.Slot);
    //     modelBuilder.Entity<Block>().HasKey(b => new { b.Id, b.Number, b.Slot });

    //     modelBuilder.Entity<TransactionOutput>(entity =>
    //     {
    //         entity.HasKey(item => new { item.Id, item.Index });

    //         entity.HasIndex(item => item.Id);
    //         entity.HasIndex(item => item.Index);
    //         entity.HasIndex(item => item.Slot);
    //         entity.HasIndex(item => item.Address);
    //         entity.HasIndex(item => item.UtxoStatus);

    //         entity.OwnsOne(item => item.Datum);

    //         entity.Ignore(item => item.AmountDatum);
    //         entity.Ignore(item => item.Amount);
    //     });

    //     modelBuilder.Entity<Transaction>().HasKey(tx => new { tx.Slot, tx.Hash });
    //     modelBuilder.Entity<Transaction>().HasIndex(tx => new { tx.Slot });

    //     modelBuilder.Entity<ReducerState>().HasKey(item => item.Name);
    //     base.OnModelCreating(modelBuilder);
    // }
}