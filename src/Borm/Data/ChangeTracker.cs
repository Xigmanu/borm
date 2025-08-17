using Borm.Model.Metadata;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace Borm.Data;

internal sealed class ChangeTracker
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly object _lock = new();

    private readonly List<Change> _changes = [];
    private readonly Dictionary<long, List<Change>> _pendingChanges = [];

    public void AcceptPendingChanges(long txId)
    {
        if (
            !_pendingChanges.TryGetValue(txId, out List<Change>? pendingChanges)
            || pendingChanges == null
        )
        {
            return;
        }

        try
        {
            lock (_lock)
            {
                List<Change> merged = ChangeMerger.Merge(_changes, pendingChanges);
                _changes.Clear();
                _changes.AddRange(merged);
            }
        }
        finally
        {
            _ = _pendingChanges.Remove(txId);
        }
    }

    public IImmutableList<Change> GetChanges()
    {
        lock (_lock)
        {
            return _changes.ToImmutableList();
        }
    }

    public void MarkChangesAsWritten()
    {
        _changes.RemoveAll(change => change.RowAction == RowAction.Delete);
        foreach (Change change in _changes)
        {
            change.MarkAsWritten();
        }
    }

    public void PendChange(Change change)
    {
        long writeTxId = change.WriteTxId;
        if (!_pendingChanges.TryGetValue(writeTxId, out List<Change>? pendingChanges))
        {
            pendingChanges = [.. _changes];
            _pendingChanges[writeTxId] = pendingChanges;
        }

        Change? existing = pendingChanges.FirstOrDefault(existing =>
            existing.Buffer.PrimaryKey.Equals(change.Buffer.PrimaryKey)
        );
        if (existing == null)
        {
            pendingChanges.Add(change);
            return;
        }

        Change? merged = existing.Merge(change);
        pendingChanges.Remove(existing);
        if (merged != null)
        {
            pendingChanges.Add(merged);
        }
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
        if (_pendingChanges.TryGetValue(txId, out List<Change>? pendingChanges))
        {
            return pendingChanges.FirstOrDefault(change => predicate(change.Buffer));
        }

        return _changes.FirstOrDefault(change => predicate(change.Buffer));
    }
}
