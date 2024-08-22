using PallasDotnet.Models;
using TxOutput = Cardano.Sync.Data.Models.Datums.TransactionOutput;
using TransactionOutputEntity = Cardano.Sync.Data.Models.TransactionOutput;
using TransactionOutput = PallasDotnet.Models.TransactionOutput;
using ValueEntity = Cardano.Sync.Data.Models.Value;
using DatumEntity = Cardano.Sync.Data.Models.Datum;
using Cardano.Sync.Data.Models.Datums;
using ValueDatum = Cardano.Sync.Data.Models.Datums.Value;
using Value = Cardano.Sync.Data.Models.Value;
using Cardano.Sync.Data.Models.Enums;
using Cardano.Sync.Data.Models;

namespace Cardano.Sync.Utils;

public static class DataUtils
{
    public static TransactionOutputEntity MapTransactionOutputEntity(string TransactionId, ulong slot, TransactionOutput output, UtxoStatus status)
    {   
        TxOutput raw = CborConverter.Deserialize<TxOutput>(output.Raw);
        return new TransactionOutputEntity
        {
            Id = TransactionId,
            Address = output.Address.ToBech32(),
            Slot = slot,
            Index = Convert.ToUInt32(output.Index),
            Datum = output.Datum is null ? null : new DatumEntity((Data.Models.DatumType)output.Datum.Type, output.Datum.Data),
            Amount = new ValueEntity
            {
                Coin = output.Amount.Coin,
                MultiAsset = output.Amount.MultiAsset.ToDictionary(
                    k => k.Key.ToHex(),
                    v => v.Value.ToDictionary(
                        k => k.Key.ToHex(),
                        v => v.Value
                    )
                )
            },
            ReferenceScript = raw?.ScriptRef,
            UtxoStatus = status,
        };
    }

    public static Value ConvertValueDatumToValue(ValueDatum valueDatum)
    {
        Dictionary<string, Dictionary<string, ulong>> multiAsset = valueDatum.MultiAsset?.Assets
            .ToDictionary(
                kvp => Convert.ToHexString(kvp.Key),
                kvp => kvp.Value.Bundle.ToDictionary(
                    assetKvp => Convert.ToHexString(assetKvp.Key),
                    assetKvp => assetKvp.Value
                )
            ) ?? [];

        return new Value
        {
            Coin = valueDatum.Lovelace,
            MultiAsset = multiAsset
        };
    }

    public static ValueDatum ConvertValueToValueDatum(Value value)
    {
        MultiAsset<ulong>? multiAssets = value.MultiAsset is not null
            ? new MultiAsset<ulong>(
                value.MultiAsset.ToDictionary(
                    kvp => Convert.FromHexString(kvp.Key),
                    kvp => new TokenBundle<ulong>(
                        kvp.Value.ToDictionary(
                            assetKvp => Convert.FromHexString(assetKvp.Key),
                            assetKvp => assetKvp.Value
                        )
                    )
                )
            )
            : null;

        return new ValueDatum(
            value.Coin,
            multiAssets
        );
    }
}