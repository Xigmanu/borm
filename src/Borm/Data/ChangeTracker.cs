using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Model.Metadata;

namespace Borm.Data;

internal sealed class ChangeTracker
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly object _lock = new();

    private readonly ChangeSet _changeSet = [];
    private readonly Dictionary<long, ChangeSet> _txChangeSets = [];

    public void AcceptPendingChanges(long txId)
    {
        if (!_txChangeSets.TryGetValue(txId, out ChangeSet? pendingSet))
        {
            return;
        }

        try
        {
            lock (_lock)
            {
                ChangeSet merged = ChangeSet.Merge(_changeSet, pendingSet);
                _changeSet.ReplaceRange(merged);
            }
        }
        finally
        {
            _ = _txChangeSets.Remove(txId);
        }
    }

    public IImmutableList<Change> Changes
    {
        get
        {
            lock (_lock)
            {
                return _changeSet.ToImmutableList();
            }
        }
    }

    public void MarkChangesAsWritten()
    {
        _changeSet.MarkAsWritten();
    }

    public void PendChange(Change change)
    {
        long writeTxId = change.WriteTxId;
        if (!_txChangeSets.TryGetValue(writeTxId, out ChangeSet? pendingSet))
        {
            pendingSet = [.. _changeSet];
            _txChangeSets[writeTxId] = pendingSet;
        }

        pendingSet.Add(change);
    }

    public bool TryGetChange(object primaryKey, long txId, [NotNullWhen(true)] out Change? change)
    {
        change = FindChange(txId, (buffer) => buffer.PrimaryKey.Equals(primaryKey));
        return change != null;
    }

    public bool IsColumnValueUnique(ColumnMetadata column, object columnValue, long txId)
    {
        return FindChange(txId, (buffer) => buffer[column].Equals(columnValue)) == null;
    }

    private Change? FindChange(long txId, Func<ValueBuffer, bool> predicate)
    {
        if (_txChangeSets.TryGetValue(txId, out ChangeSet? pendingSet))
        {
            return pendingSet.FirstOrDefault(change => predicate(change.Buffer));
        }

        return _changeSet.FirstOrDefault(change => predicate(change.Buffer));
    }
}
