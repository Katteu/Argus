namespace Cardano.Sync.Data.Models;

public record Transaction
{
    public ulong Slot { get; set; }
    public string Hash { get; set; } = default!;
    public byte[] TxCbor { get; set; } = [];
}