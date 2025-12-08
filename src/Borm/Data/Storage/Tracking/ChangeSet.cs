using System.Collections;
using System.Diagnostics;
using Borm.Properties;

namespace Borm.Data.Storage.Tracking;

[DebuggerDisplay("Count = {Count}")]
internal sealed class ChangeSet : IEnumerable<IChange>
{
    private readonly Dictionary<object, IChange> _changes;
    private readonly HashSet<object> _danglingKeys;
    private readonly IMerger _merger;

    public ChangeSet(IMerger merger)
        : this([], [], merger)
    {
    }

    private ChangeSet(
        Dictionary<object, IChange> changePkMap,
        HashSet<object> danglingKeyCache,
        IMerger merger
    )
    {
        _changes = changePkMap;
        _danglingKeys = danglingKeyCache;
        _merger = merger;
    }

    public int Count => _changes.Count;

    public IEnumerator<IChange> GetEnumerator()
    {
        return _changes.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    internal event EventHandler<RecordRemovedEventArgs>? RecordRemoved;

    public ChangeSet MergeWith(ChangeSet incoming)
    {
        if (incoming.Count == 0)
        {
            // Assume that all changes have been deleted
            return new ChangeSet(_changes, incoming._danglingKeys, _merger);
        }

        Dictionary<object, IChange> resultMap = new(_changes);
        foreach ((object primaryKey, IChange incomingChange) in incoming._changes)
        {
            if (_changes.TryGetValue(primaryKey, out IChange? existingChange))
            {
                IChange? merged = _merger.Merge(existingChange, incomingChange, MergeMode.Commit);
                resultMap.Remove(primaryKey);
                if (merged != null)
                {
                    resultMap[primaryKey] = merged;
                }
                else
                {
                    RaiseRecordRemoved(primaryKey);
                }
            }
            else
            {
                if (
                    incoming._danglingKeys.Contains(primaryKey)
                    || incomingChange.RowAction != RowAction.Insert
                    && incomingChange.WriteId != Transaction.InitId
                )
                {
                    throw new InvalidOperationException(Strings.ModificationOfNonExistingRow());
                }

                resultMap[primaryKey] = incomingChange;
            }
        }

        return new ChangeSet(resultMap, [.. incoming._danglingKeys], _merger);
    }

    public void Add(IChange incoming)
    {
        object primaryKey = incoming.Record.PrimaryKey;
        if (
            _changes.TryGetValue(primaryKey, out IChange? existing)
            && incoming.WriteId == existing.WriteId
        )
        {
            IChange? merged = _merger.Merge(existing, incoming, MergeMode.Normal);
            _changes.Remove(primaryKey);
            if (merged is not null)
            {
                _changes[primaryKey] = merged;
            }

            return;
        }

        _changes[primaryKey] = incoming;
    }

    public ChangeSet Copy()
    {
        return new ChangeSet(
            new Dictionary<object, IChange>(_changes),
            [.. _danglingKeys],
            _merger
        );
    }

    public void MarkAsWritten()
    {
        foreach ((object primaryKey, IChange change) in _changes)
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
        foreach ((object primaryKey, IChange change) in changes._changes)
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