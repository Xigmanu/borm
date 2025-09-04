namespace Borm.Data.Storage;

public sealed class TransactionIdMismatchException : InvalidOperationException
{
    public TransactionIdMismatchException() { }

    public TransactionIdMismatchException(string message)
        : base(message) { }

    public TransactionIdMismatchException(string message, Exception? innerException)
        : base(message, innerException) { }
}
