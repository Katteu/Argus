using System.Formats.Cbor;
using CborSerialization;

namespace Cardano.Sync.Data.Models.Datums;

// {
//     0: h'70260d37efda1b192812ee454abb7ccc233d56a4e9a7eb5d41983c6eee',
//     1: [
//         1685210_2,
//         {
//             h'5249ff3b2648a51cb0a6dbc9590b715a2bd6658bbab277e3a7768d73': {h'744c51': 30000000_2},
//             h'f3ca4025742abaf468d95728c609bafefcacf0a99cab27f2b2a0ce76': {
//                 h'260d37efda1b192812ee454abb7ccc233d56a4e9a7eb5d41983c6eee': 1,
//             },
//         },
//     ],
//     2: [
//         1,
//         24_0(<<[_
//             30000000_2,
//             121_0([_
//                 h'57d744e34a00d2bcb0c2c1321881cc289cf6d6af00a83c3caf57e2be',
//             ]),
//             122_0([]),
//             [_
//                 [_ 139_0, 121_0([])],
//                 [_ 138_0, 121_0([])],
//                 [_ 137_0, 121_0([])],
//                 [_ 136_0, 121_0([])],
//                 [_ 135_0, 121_0([])],
//             ],
//         ]>>),
//     ],
// }
[CborSerialize(typeof(TxOutputCborConvert))]
public record TxOutput(byte[] OutputAddress, Value Amount, Datum Data, byte[] ScriptRef) : IDatum;

public class TxOutputCborConvert : ICborConvertor<TxOutput>
{
    public TxOutput Read(ref CborReader reader)
    {
        byte[]? outputAddress = null;
        Value? amount = null;
        Datum? data = null;
        byte[]? scriptRef = null;

        if (reader.PeekState() == CborReaderState.StartMap)
        {
            reader.ReadStartMap();
            while (reader.PeekState() != CborReaderState.EndMap)
            {
                int key = reader.ReadInt32();
                switch (key)
                {
                    case 0:
                        outputAddress = reader.ReadByteString();
                        break;
                    case 1:
                        reader.SkipValue();
                        //amount = new ValueCborConvert().Read(ref reader);
                        break;
                    case 2:
                        reader.SkipValue();
                        break;
                    case 3:
                        var tag = reader.ReadTag();
                        if ((int)tag != 24)
                        {
                            throw new Exception("Invalid tag");
                        }
                        var array = reader.ReadByteString();
                        var innerReader = new CborReader(array);
                        if (innerReader.PeekState() == CborReaderState.StartArray)
                        {
                            innerReader.ReadStartArray();
                            _ = innerReader.ReadInt32();
                            scriptRef = innerReader.ReadEncodedValue().ToArray();
                            innerReader.ReadEndArray();
                        }
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            reader.ReadEndMap();
        }
        else
        {
            reader.ReadStartArray();
            outputAddress = reader.ReadByteString();
            if (reader.PeekState() == CborReaderState.StartArray)
            {
                reader.ReadStartArray();
                reader.SkipValue();
                if (reader.PeekState() == CborReaderState.StartMap)
                {
                    reader.SkipValue();
                }
                reader.ReadEndArray();
            }
            else
            {
                reader.SkipValue();
            }

            if (reader.PeekState() != CborReaderState.EndArray)
            {
                reader.SkipValue();
            }
            reader.ReadEndArray();
        }

        return new TxOutput(outputAddress, amount, data, scriptRef);
    }

    public void Write(ref CborWriter writer, TxOutput value)
    {
        throw new NotImplementedException();
    }
}