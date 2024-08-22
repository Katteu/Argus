using System.Formats.Cbor;
using Cardano.Sync.Data.Models.Datums;
using CardanoSharp.Wallet.Models.Transactions;
using CardanoSharp.Wallet.Models.Transactions.TransactionWitness;
using CborSerialization;

namespace Cardano.Sync.Data.Models.Experimental;

[CborSerialize(typeof(TransactionCborConvert))]
public record Transaction() : IDatum;

public class TransactionCborConvert : ICborConvertor<Transaction>
{
    public Transaction Read(ref CborReader reader)
    {
        reader.ReadStartArray();
        throw new NotImplementedException();
    }

    public void Write(ref CborWriter writer, Transaction value)
    {
        throw new NotImplementedException();
    }
}