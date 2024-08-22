using Cardano.Sync.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace Cardano.Sync.Data;


public class CardanoDbContext(DbContextOptions options, IConfiguration configuration) : DbContext(options)
{
    private readonly IConfiguration _configuration = configuration;
    public DbSet<Block> Blocks { get; set; }
    public DbSet<TransactionOutput> TransactionOutputs { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ReducerState> ReducerStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_configuration.GetConnectionString("CardanoContextSchema"));
        modelBuilder.Entity<Block>().HasIndex(b => b.Slot);
        modelBuilder.Entity<Block>().HasKey(b => new { b.Id, b.Number, b.Slot });

        modelBuilder.Entity<TransactionOutput>(entity =>
        {
            entity.HasKey(item => new { item.Id, item.Index });

            entity.HasIndex(item => item.Id);
            entity.HasIndex(item => item.Index);
            entity.HasIndex(item => item.Slot);
            entity.HasIndex(item => item.Address);
            entity.HasIndex(item => item.UtxoStatus);

            entity.OwnsOne(item => item.Datum);

            entity.Ignore(item => item.AmountDatum);
            entity.Ignore(item => item.Amount);
        });

        modelBuilder.Entity<Transaction>().HasKey(tx => new { tx.Slot, tx.Hash });
        modelBuilder.Entity<Transaction>().HasIndex(tx => new { tx.Slot });

        modelBuilder.Entity<ReducerState>().HasKey(item => item.Name);
        base.OnModelCreating(modelBuilder);
    }
}