using System.ComponentModel.DataAnnotations.Schema;
using PeterO.Cbor2;

namespace Cardano.Sync.Data.Models;

public record Value
{
    public ulong Coin { get; init; } = default!;

    [NotMapped]
    public Dictionary<string, Dictionary<string, ulong>> MultiAsset { get; set; } = default!;
    
    public byte[] MultiAssetCbor
    {
        get
        {
            CBORObject cborObject = CBORObject.FromObject(MultiAsset);
            return cborObject.EncodeToBytes();
        }

        set
        {
            if (value == null || value.Length == 0)
            {
                MultiAsset = [];
            }
            else
            {
                CBORObject cborObject = CBORObject.DecodeFromBytes(value);
                MultiAsset = cborObject.ToObject<Dictionary<string, Dictionary<string, ulong>>>() ?? [];
            }
        }
    }
}