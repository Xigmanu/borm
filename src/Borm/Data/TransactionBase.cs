namespace Borm.Data;

public abstract class TransactionBase : IDisposable
{
    protected const int MaxRetries = 3;
    protected readonly long id;
    protected readonly Queue<(Action<object, long>, object)> operationQueue;
    protected int attempt;
    protected Exception? exception;
    private bool _isDisposed;

    protected TransactionBase()
    {
        id = IdProvider.Next();
        exception = null;
        operationQueue = [];
        _isDisposed = false;
        attempt = 0;
    }

    protected TransactionBase(TransactionBase original)
    {
        id = IdProvider.Next();
        exception = null;
        operationQueue = original.operationQueue;
        _isDisposed = false;
        attempt = original.attempt++;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal virtual void Execute(Action<object, long> tableOperation, object entity)
    {
        if (exception != null)
        {
            return;
        }

        try
        {
            operationQueue.Enqueue((tableOperation, entity));
            tableOperation(entity, id);
        }
        catch (Exception ex)
        {
            exception = ex;
        }
    }

    protected abstract void CommitPendingChanges();

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (disposing)
        {
            CommitPendingChanges();
        }
    }

    protected void RunQueuedOperations()
    {
        for (int i = 0; i < operationQueue.Count; i++)
        {
            (Action<object, long> operation, object entity) = operationQueue.Dequeue();
            Execute(operation, entity);
        }
    }

    private static class IdProvider
    {
        private static long _counter;

        internal static long Next()
        {
            return Interlocked.Increment(ref _counter);
        }
    }
}
