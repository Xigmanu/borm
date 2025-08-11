using System.Collections.Immutable;
using System.Diagnostics;
using Borm.Model.Metadata;

namespace Borm.Data;

[DebuggerDisplay("Changes={_changes}")]
internal sealed class ChangeTracker
{
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
                List<Change> merged = Merge(_changes, pendingChanges);
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

    public bool HasChange(object primaryKey, long txId)
    {
        return HasChange(txId, (buffer) => buffer.GetPrimaryKey().Equals(primaryKey));
    }

    public bool HasChange(ColumnInfo column, object columnValue, long txId)
    {
        return HasChange(txId, (buffer) => buffer[column].Equals(columnValue));
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
        long txId = change.TxId;
        if (!_pendingChanges.TryGetValue(txId, out List<Change>? pendingChanges))
        {
            pendingChanges = [.. _changes]; //TODO Initial row ids has to be retained up until a commit
            _pendingChanges[txId] = pendingChanges;
        }

        Change? existing = pendingChanges.FirstOrDefault(existing =>
            existing.Buffer.GetPrimaryKey().Equals(change.Buffer.GetPrimaryKey())
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

    private static List<Change> Merge(List<Change> original, List<Change> incoming)
    {
        Dictionary<object, Change> resultMap = original
            .GroupBy(c => c.Buffer.GetPrimaryKey())
            .ToDictionary(g => g.Key, g => g.First());

        foreach (Change change in incoming)
        {
            object key = change.Buffer.GetPrimaryKey();
            if (resultMap.TryGetValue(key, out Change? existing))
            {
                Change? merged = existing.Merge(change);
                if (merged != null)
                {
                    resultMap[key] = merged;
                }
                else
                {
                    resultMap.Remove(key);
                }
            }
            else
            {
                if (change.RowAction != RowAction.Insert)
                {
                    throw new Exception("Oh no, stinky");
                }
                resultMap[key] = change;
            }
        }

        return [.. resultMap.Values];
    }

    private bool HasChange(long txId, Func<ValueBuffer, bool> predicate)
    {
        if (_pendingChanges.TryGetValue(txId, out List<Change>? pendingChanges))
        {
            return pendingChanges.Any(change => predicate(change.Buffer));
        }

        return _changes.Any(change => predicate(change.Buffer));
    }
}
