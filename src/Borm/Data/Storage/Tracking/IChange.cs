namespace Borm.Data.Storage.Tracking;

internal interface IChange
{
    bool IsWrittenToDataSource { get; }
    long ReadId { get; }
    IValueBuffer Record { get; }
    OperationKind Operation { get; }
    long WriteId { get; }
    void MarkAsWritten();
}