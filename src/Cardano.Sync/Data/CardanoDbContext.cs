using Cardano.Sync.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace Cardano.Sync.Data;


public class CardanoDbContext(DbContextOptions options, IConfiguration configuration) : DbContext(options)
{
    private readonly IConfiguration _configuration = configuration;
    public DbSet<Block> Blocks { get; set; }
    public DbSet<ReducerState> ReducerStates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(_configuration.GetConnectionString("CardanoContextSchema"));

        modelBuilder.Entity<Block>().HasIndex(b => b.Slot);
        modelBuilder.Entity<Block>().HasKey(b => new { b.Id, b.Number, b.Slot });

        modelBuilder.Entity<ReducerState>().HasKey(item => item.Name);

        base.OnModelCreating(modelBuilder);
    }
}