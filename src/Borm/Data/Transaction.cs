using System.Diagnostics;
using Borm.Data.Storage;
using Borm.Properties;

namespace Borm.Data;

/// <summary>
/// Represents a transactional scope for write operations performed through a <see cref="DataContext"/>.
/// </summary>
///
/// <remarks>
///     <para>
///         If any of the entries affected by this transaction have been changed by another transaction, the operations will be re-executed in the same order.
///     </para>
///     <para>
///         Created by calling <see cref="DataContext.BeginTransaction"/>.
///     </para>
/// </remarks>
public sealed class Transaction : IDisposable
{
    internal const long InitId = -1;
    private const int MaxRetries = 3;

    private readonly HashSet<Table> _changedTables;
    private readonly TableGraph _graph;
    private readonly long _id;
    private readonly Queue<Action<long, HashSet<Table>>> _operationQueue;

    private int _attempt;
    private Exception? _exception;

    internal Transaction(TableGraph graph)
        : this(IdProvider.Next(), graph) { }

    internal Transaction(long id, TableGraph graph)
    {
        this._id = id;
        _exception = null;
        _graph = graph;
        _operationQueue = [];
        _changedTables = [];
        _attempt = 0;
    }

    private Transaction(Transaction original)
    {
        _id = IdProvider.Next();
        _exception = null;
        _graph = original._graph;
        _operationQueue = original._operationQueue;
        _changedTables = original._changedTables;
        _attempt = ++original._attempt;
    }

    public void Dispose()
    {
        CommitPendingChanges();
    }

    internal void Execute(Action<long, HashSet<Table>> tableOperation)
    {
        if (_exception != null)
        {
            return;
        }

        try
        {
            _operationQueue.Enqueue(tableOperation);
            tableOperation(_id, _changedTables);
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
    }

    private void CommitPendingChanges()
    {
        if (_exception != null)
        {
            Debug.Assert(_exception is not ConcurrencyConflictException);
            throw new InvalidOperationException(Strings.TransactionFailed(), _exception);
        }

        try
        {
            HashSet<Table> processed = [];
            foreach (Table changedTable in _changedTables)
            {
                List<Table> tables = [.. _graph.GetParents(changedTable), changedTable];
                foreach (Table table in tables.Where(processed.Add))
                {
                    table.Tracker.AcceptPendingChanges(_id);
                }
            }
        }
        catch (ConcurrencyConflictException ex)
        {
            if (_attempt >= MaxRetries)
            {
                throw new InvalidOperationException(Strings.TransactionMaxRerunsExceeded(), ex);
            }

            using Transaction retryTx = new(this);
            retryTx.RunQueuedOperations();
        }
    }

    private void RunQueuedOperations()
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
