namespace Borm.Data.Storage.Tracking;

internal sealed class Change : IChange
{
    public Change(
        IValueBuffer record,
        long readTxId,
        long writeTxId,
        bool isWrittenToDataSource,
        OperationKind operation
    )
    {
        Record = record;
        ReadId = readTxId;
        WriteId = writeTxId;
        IsWrittenToDataSource = isWrittenToDataSource;
        Operation = operation;
    }

    public bool IsWrittenToDataSource { get; }
    public long ReadId { get; }
    public IValueBuffer Record { get; }
    public OperationKind Operation { get; }
    public long WriteId { get; }

    public IChange MarkAsCommittedToDataSource()
    {
        return new Change(Record, ReadId, WriteId, isWrittenToDataSource: true, OperationKind.None);
    }

    public override bool Equals(object? obj)
    {
        return obj is Change other && Record.PrimaryKey.Equals(other.Record.PrimaryKey);
    }

    public override int GetHashCode()
    {
        return Record.PrimaryKey.GetHashCode();
    }
}