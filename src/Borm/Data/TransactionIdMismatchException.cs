namespace Borm.Data;

public sealed class TransactionIdMismatchException : InvalidOperationException
{
    public TransactionIdMismatchException(string message, long currentTxId, long incomingTxId)
        : base(message)
    {
        CurrentTxId = currentTxId;
        IncomingTxId = incomingTxId;
    }

    public long CurrentTxId { get; }
    public long IncomingTxId { get; }
}
