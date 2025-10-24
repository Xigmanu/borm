namespace Borm.Data.Storage.Tracking;

internal sealed class Change : IChange
{
    public Change(
        IValueBuffer record,
        long readTxId,
        long writeTxId,
        bool isWrittenToDataSource,
        RowAction rowAction
    )
    {
        Record = record;
        ReadId = readTxId;
        WriteId = writeTxId;
        IsWrittenToDataSource = isWrittenToDataSource;
        RowAction = rowAction;
    }

    public bool IsWrittenToDataSource { get; private set; }
    public long ReadId { get; }
    public IValueBuffer Record { get; }
    public RowAction RowAction { get; private set; }
    public long WriteId { get; }

    public override bool Equals(object? obj)
    {
        return obj is Change other && Record.PrimaryKey.Equals(other.Record.PrimaryKey);
    }

    public override int GetHashCode()
    {
        return Record.PrimaryKey.GetHashCode();
    }

    public void MarkAsWritten()
    {
        IsWrittenToDataSource = true;
        RowAction = RowAction.None;
    }
}
