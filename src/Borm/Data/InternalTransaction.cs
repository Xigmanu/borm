using System.Diagnostics;
using Borm.Data.Sql;
using Borm.Data.Storage;
using Borm.Properties;

namespace Borm.Data;

/// <summary>
/// Represents an internal transactional scope for direct write operations through a <see cref="DataContext"/>.
/// </summary>
public class InternalTransaction : IDisposable
{
    internal const long InitId = -1;

    protected readonly long id;
    protected Exception? exception;

    private const int MaxRetries = 3;

    private readonly List<Table> _changedTables;
    private readonly Queue<Action<long>> _operationQueue;
    private int _attempt;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _isDisposed;

    internal InternalTransaction()
        : this(IdProvider.Next()) { }

    internal InternalTransaction(long id)
    {
        this.id = id;
        exception = null;
        _operationQueue = [];
        _changedTables = [];
        _isDisposed = false;
        _attempt = 0;
    }

    protected InternalTransaction(InternalTransaction original)
    {
        id = IdProvider.Next();
        exception = null;
        _operationQueue = original._operationQueue;
        _changedTables = original._changedTables;
        _isDisposed = false;
        _attempt = ++original._attempt;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    internal virtual void Execute(Table table, Action<long> tableOperation)
    {
        if (TryExecute(tableOperation))
        {
            _changedTables.Add(table);
        }
    }

    protected virtual void CommitPendingChanges()
    {
        if (exception != null)
        {
            Debug.Assert(exception is not ConcurrencyConflictException);
            throw new InvalidOperationException(Strings.TransactionFailed(), exception);
        }

        try
        {
            foreach (Table changedTable in _changedTables)
            {
                changedTable.AcceptPendingChanges(id);
            }
        }
        catch (ConcurrencyConflictException ex)
        {
            if (_attempt >= MaxRetries)
            {
                throw new InvalidOperationException(Strings.TransactionMaxRerunsExceeded(), ex);
            }

            using InternalTransaction retryTx = new(this);
            retryTx.RunQueuedOperations();
        }
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
        for (int i = 0; i < _operationQueue.Count; i++)
        {
            Action<long> operation = _operationQueue.Dequeue();
            _ = TryExecute(operation);
        }
    }

    [Conditional("DEBUG")]
    private static void AssertArgumentTypeIsValid(object arg)
    {
        Type type = arg.GetType();
        Debug.Assert(type == typeof(ValueBuffer) || type == typeof(ResultSet));
    }

    private bool TryExecute(Action<long> tableOperation)
    {
        if (exception != null)
        {
            return false;
        }

        try
        {
            _operationQueue.Enqueue(tableOperation);
            tableOperation(id);
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
