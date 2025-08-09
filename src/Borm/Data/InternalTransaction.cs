namespace Borm.Data;

public class InternalTransaction : IDisposable
{
    protected readonly long id;
    protected Exception? exception;
    private const int MaxRetries = 3;
    private readonly List<Table> _changedTables;
    private readonly Queue<(Action<object, long>, object)> operationQueue;
    private bool _isDisposed;
    private int _attempt;

    internal InternalTransaction()
    {
        id = IdProvider.Next();
        exception = null;
        operationQueue = [];
        _changedTables = [];
        _isDisposed = false;
        _attempt = 0;
    }

    protected InternalTransaction(InternalTransaction original)
    {
        id = IdProvider.Next();
        exception = null;
        operationQueue = original.operationQueue;
        _changedTables = original._changedTables;
        _isDisposed = false;
        _attempt = ++original._attempt;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal virtual void Execute(Table table, Action<object, long> tableOperation, object entity)
    {
        if (Execute(tableOperation, entity))
        {
            _changedTables.Add(table);
        }
    }

    protected virtual void CommitPendingChanges()
    {
        if (exception == null)
        {
            _changedTables.ForEach(table => table.AcceptPendingChanges(id));
            return;
        }
        if (exception is not TransactionIdMismatchException)
        {
            throw new Exception("TODO", exception);
        }
        if (_attempt >= MaxRetries)
        {
            throw new Exception();
        }

        using InternalTransaction retryTx = new(this);
        retryTx.RunQueuedOperations();
    }

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
            _ = Execute(operation, entity);
        }
    }

    private bool Execute(Action<object, long> tableOperation, object entity)
    {
        if (exception != null)
        {
            return false;
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

        return true;
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
