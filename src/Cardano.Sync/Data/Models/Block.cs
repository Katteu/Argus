namespace Cardano.Sync.Data.Models;

public record Block
{
    public string Id { get; set; } = default!;
    public ulong Number { get; set; }
    public ulong Slot { get; set; }
    public byte[] BlockCbor { get; set; } = [];
}