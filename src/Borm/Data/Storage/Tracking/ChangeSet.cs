using System.Collections;
using System.Diagnostics;
using Borm.Properties;

namespace Borm.Data.Storage.Tracking;

[DebuggerDisplay("Count = {Count}")]
internal sealed class ChangeSet : IEnumerable<Change>
{
    private readonly Dictionary<object, Change> _changes;
    private readonly HashSet<object> _danglingKeys;

    public ChangeSet()
    {
        (_changes, _danglingKeys) = ([], []);
    }

    private ChangeSet(Dictionary<object, Change> changePkMap, HashSet<object> danglingKeyCache)
    {
        _changes = changePkMap;
        _danglingKeys = danglingKeyCache;
    }

    internal event EventHandler<RecordRemovedEventArgs>? RecordRemoved;
    public int Count => _changes.Count;

    public static ChangeSet Merge(ChangeSet existing, ChangeSet incoming)
    {
        if (incoming.Count == 0)
        {
            // Assume that all changes have been deleted
            return new ChangeSet(existing._changes, incoming._danglingKeys);
        }

        Dictionary<object, Change> resultMap = new(existing._changes);
        foreach ((object primaryKey, Change incomingChange) in incoming._changes)
        {
            if (existing._changes.TryGetValue(primaryKey, out Change? existingChange))
            {
                Change? merged = existingChange.CommitMerge(incomingChange);
                resultMap.Remove(primaryKey);
                if (merged != null)
                {
                    resultMap[primaryKey] = merged;
                }
                else
                {
                    existing.RaiseRecordRemoved(primaryKey);
                }
            }
            else
            {
                if (
                    incoming._danglingKeys.Contains(primaryKey)
                    || incomingChange.RowAction != RowAction.Insert
                        && incomingChange.WriteTxId != InternalTransaction.InitId
                )
                {
                    throw new InvalidOperationException(Strings.ModificationOfNonExistingRow());
                }
                resultMap[primaryKey] = incomingChange;
            }
        }

        return new ChangeSet(resultMap, [.. incoming._danglingKeys]);
    }

    public void Add(Change incoming)
    {
        object primaryKey = incoming.Buffer.PrimaryKey;
        if (
            _changes.TryGetValue(primaryKey, out Change? existing)
            && incoming.WriteTxId == existing.WriteTxId
        )
        {
            Change? merged = existing.Merge(incoming);
            _changes.Remove(primaryKey);
            if (merged is not null)
            {
                _changes[primaryKey] = merged;
            }
            return;
        }
        _changes[primaryKey] = incoming;
    }

    public IEnumerator<Change> GetEnumerator()
    {
        return _changes.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void MarkAsWritten()
    {
        foreach ((object primaryKey, Change change) in _changes)
        {
            if (change.RowAction == RowAction.Delete)
            {
                _changes.Remove(primaryKey);
            }
            change.MarkAsWritten();
        }
        _danglingKeys.Clear();
    }

    public void ReplaceRange(ChangeSet changes)
    {
        _changes.Clear();
        foreach ((object primaryKey, Change change) in changes._changes)
        {
            _changes[primaryKey] = change;
        }

        _danglingKeys.UnionWith(changes._danglingKeys);
    }

    internal void OnRecordRemoved(object? sender, RecordRemovedEventArgs e)
    {
        _danglingKeys.Add(e.PrimaryKey);
    }

    private void RaiseRecordRemoved(object primaryKey)
    {
        RecordRemoved?.Invoke(this, new RecordRemovedEventArgs(primaryKey));
    }
}
