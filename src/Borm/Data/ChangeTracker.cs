using System.Collections.Immutable;
using System.Diagnostics;
using Borm.Model.Metadata;

namespace Borm.Data;

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
            List<Change> merged = Merge(_changes, pendingChanges);
            lock (_lock)
            {
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
        return HasChange(txId, (buffer) => buffer.GetPrimaryKey() == primaryKey);
    }

    public bool HasChange(ColumnInfo column, object? columnValue, long txId)
    {
        return HasChange(txId, (buffer) => buffer[column] == columnValue);
    }

    public void LoadFromDataSource(ValueBuffer buffer, long txId)
    {
        Debug.Assert(txId == -1);
        Change entry = new(buffer, txId, RowAction.None, isWrittenToDb: true);
        _changes.Add(entry);
    }

    public void MarkChangesAsWritten()
    {
        _changes.ForEach(change =>
        {
            if (!change.IsWrittenToDb)
            {
                change.IsWrittenToDb = true;
            }
        });
    }

    public void PendChange(Change change, long txId)
    {
        if (!_pendingChanges.TryGetValue(txId, out List<Change>? pendingChanges))
        {
            _pendingChanges[txId] = [change];
            return;
        }
        Debug.Assert(pendingChanges != null);

        Change? existing = pendingChanges.FirstOrDefault(existing =>
            existing.Buffer.GetPrimaryKey() == change.Buffer.GetPrimaryKey()
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
