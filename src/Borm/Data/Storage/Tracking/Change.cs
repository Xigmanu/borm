namespace Borm.Data.Storage.Tracking;

internal sealed class Change
{
    public Change(
        ValueBuffer buffer,
        long readTxId,
        long writeTxId,
        bool isWrittenToDb,
        RowAction rowAction
    )
    {
        Buffer = buffer;
        ReadTxId = readTxId;
        WriteTxId = writeTxId;
        IsWrittenToDb = isWrittenToDb;
        RowAction = rowAction;
    }

    public bool IsWrittenToDb { get; private set; }
    public long ReadTxId { get; }
    public RowAction RowAction { get; private set; }
    public long WriteTxId { get; }
    internal ValueBuffer Buffer { get; }

    public static Change Initial(ValueBuffer buffer, long txId)
    {
        return new Change(buffer, txId, txId, isWrittenToDb: true, RowAction.None);
    }

    public static Change NewChange(ValueBuffer buffer, long txId)
    {
        return new Change(buffer, txId, txId, isWrittenToDb: false, RowAction.Insert);
    }

    public Change Delete(ValueBuffer buffer, long writeTxId)
    {
        return new Change(buffer, ReadTxId, writeTxId, IsWrittenToDb, RowAction.Delete);
    }

    public override bool Equals(object? obj)
    {
        return obj is Change other && Buffer.PrimaryKey.Equals(other.Buffer.PrimaryKey);
    }

    public override int GetHashCode()
    {
        return Buffer.PrimaryKey.GetHashCode();
    }

    public void MarkAsWritten()
    {
        IsWrittenToDb = true;
        RowAction = RowAction.None;
    }

    public Change Update(ValueBuffer buffer, long writeTxId)
    {
        return new Change(buffer, ReadTxId, writeTxId, IsWrittenToDb, RowAction.Update);
    }
}
