using Cardano.Sync.Data.Models.Enums;
using ValueDatum = Cardano.Sync.Data.Models.Datums.Value;
using Cardano.Sync.Utils;

namespace Cardano.Sync.Data.Models;

public record TransactionOutput
{
    public string Id { get; init; } = default!;
    public uint Index { get; init; }
    public ulong Slot { get; init; }
    public ulong? SpentSlot { get; set; }
    public string Address { get; init; } = default!;
    public byte[] AmountCbor { get; private set; } = [];
    public Datum? Datum { get; init; }
    public byte[]? ReferenceScript { get; init; }
    public UtxoStatus UtxoStatus { get; set; }
 
    public ValueDatum AmountDatum
    {
        get => CborConverter.Deserialize<ValueDatum>(AmountCbor);
        set => AmountCbor = CborConverter.Serialize(value);
    }

    public Value Amount
    {
        get => DataUtils.ConvertValueDatumToValue(AmountDatum);
        set => AmountDatum = DataUtils.ConvertValueToValueDatum(value);
    }
}