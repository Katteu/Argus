using PallasDotnet.Models;
// using TxOutput = Cardano.Sync.Data.Models.Datums.TransactionOutput;
// using TransactionOutputEntity = Cardano.Sync.Data.Models.TransactionOutput;
// using TransactionOutput = PallasDotnet.Models.TransactionOutput;

namespace Cardano.Sync.Utils;

public static class DataUtils
{
    // public static TransactionOutputEntity MapTransactionOutputEntity(string TransactionId, ulong slot, TransactionOutput output, UtxoStatus status)
    // {   
    //     TxOutput raw = CborConverter.Deserialize<TxOutput>(output.Raw);
    //     return new TransactionOutputEntity
    //     {
    //         Id = TransactionId,
    //         Address = output.Address.ToBech32(),
    //         Slot = slot,
    //         Index = Convert.ToUInt32(output.Index),
    //         Datum = output.Datum is null ? null : new DatumEntity((Data.Models.DatumType)output.Datum.Type, output.Datum.Data),
    //         Amount = new ValueEntity
    //         {
    //             Coin = output.Amount.Coin,
    //             MultiAsset = output.Amount.MultiAsset.ToDictionary(
    //                 k => k.Key.ToHex(),
    //                 v => v.Value.ToDictionary(
    //                     k => k.Key.ToHex(),
    //                     v => v.Value
    //                 )
    //             )
    //         },
    //         ReferenceScript = raw?.ScriptRef,
    //         UtxoStatus = status,
    //     };
    // }
}