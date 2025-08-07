using System.Collections.Immutable;
using System.Diagnostics;
using Borm.Model.Metadata;

namespace Borm.Data;

internal sealed class ChangeTracker
{
    private static readonly object _lock = new();

    private readonly List<Change> _changes = [];
    private readonly Dictionary<long, List<Change>> _pendingChanges = [];

    // TODO!!!
    // 2. Rework transaction logic: add implicit (internal) and explicit (public) transactions for easier row insert tx id tracking

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

    public bool HasRow(object primaryKey)
    {
        return _changes.Any(change => change.Buffer.GetPrimaryKey() == primaryKey);
    }

    public void InitInsert(ValueBuffer buffer, long txId)
    {
        Debug.Assert(txId == -1);
        Change entry = new(buffer, txId, RowAction.None);
        _changes.Add(entry);
    }

    public void PendUpdate(Change change, long txId)
    {
        if (!_pendingChanges.TryGetValue(txId, out List<Change>? pendingChanges))
        {
            _pendingChanges[txId] = [change];
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
}
