using System.Diagnostics;
using System.Linq;
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

    private readonly HashSet<Table> _changedTables;
    private readonly Queue<Action<long, HashSet<Table>>> _operationQueue;
    private readonly TableGraph _graph;
    private int _attempt;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _isDisposed;

    internal InternalTransaction(TableGraph graph)
        : this(IdProvider.Next(), graph) { }

    internal InternalTransaction(long id, TableGraph graph)
    {
        this.id = id;
        exception = null;
        _graph = graph;
        _operationQueue = [];
        _changedTables = [];
        _isDisposed = false;
        _attempt = 0;
    }

    protected InternalTransaction(InternalTransaction original)
    {
        id = IdProvider.Next();
        exception = null;
        _graph = original._graph;
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

    internal virtual void Execute(Action<long, HashSet<Table>> tableOperation)
    {
        if (exception != null)
        {
            return;
        }

        try
        {
            _operationQueue.Enqueue(tableOperation);
            tableOperation(id, _changedTables);
        }
        catch (Exception ex)
        {
            exception = ex;
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
            HashSet<Table> processed = [];
            foreach (Table changedTable in _changedTables)
            {
                List<Table> tables = [.. _graph.GetParents(changedTable), changedTable];
                foreach (Table table in tables.Where(processed.Add))
                {
                    table.Tracker.AcceptPendingChanges(id);
                }
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
            Action<long, HashSet<Table>> operation = _operationQueue.Dequeue();
            Execute(operation);
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
