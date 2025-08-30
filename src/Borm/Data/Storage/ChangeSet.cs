using System.Collections;
using System.Diagnostics;
using Borm.Properties;

namespace Borm.Data.Storage;

[DebuggerDisplay("Count = {_changePKMap.Count}")]
internal sealed class ChangeSet : IEnumerable<Change>
{
    private readonly Dictionary<object, Change> _changePKMap;

    public ChangeSet()
    {
        _changePKMap = [];
    }

    private ChangeSet(Dictionary<object, Change> changePkMap)
    {
        _changePKMap = changePkMap;
    }

    public int Count => _changePKMap.Count;

    public static ChangeSet Merge(ChangeSet existing, ChangeSet incoming)
    {
        if (incoming.Count == 0)
        {
            // Assume that all changes have been deleted
            return [];
        }

        Dictionary<object, Change> resultMap = new(existing._changePKMap);
        foreach ((object incomingPk, Change incomingChange) in incoming._changePKMap)
        {
            if (existing._changePKMap.TryGetValue(incomingPk, out Change? existingChange))
            {
                Change? merged = existingChange.Merge(incomingChange);
                resultMap.Remove(incomingPk);
                if (merged != null)
                {
                    resultMap[incomingPk] = merged;
                }
            }
            else
            {
                if (
                    incomingChange.RowAction != RowAction.Insert
                    && incomingChange.WriteTxId != InternalTransaction.InitId
                )
                {
                    throw new InvalidOperationException(Strings.ModificationOfNonExistingRow());
                }
                resultMap[incomingPk] = incomingChange;
            }
        }
        return new ChangeSet(resultMap);
    }

    public void Add(Change incoming)
    {
        object primaryKey = incoming.Buffer.PrimaryKey;
        if (_changePKMap.TryGetValue(primaryKey, out Change? existing))
        {
            Change? merged = existing.Merge(incoming);
            _changePKMap.Remove(primaryKey);
            if (merged != null)
            {
                _changePKMap[primaryKey] = merged;
            }
            return;
        }

        _changePKMap[primaryKey] = incoming;
    }

    public IEnumerator<Change> GetEnumerator()
    {
        return _changePKMap.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void MarkAsWritten()
    {
        foreach ((object primaryKey, Change change) in _changePKMap)
        {
            if (change.RowAction == RowAction.Delete)
            {
                _changePKMap.Remove(primaryKey);
            }
            change.MarkAsWritten();
        }
    }

    public void ReplaceRange(ChangeSet changes)
    {
        _changePKMap.Clear();
        foreach ((object primaryKey, Change change) in changes._changePKMap)
        {
            _changePKMap[primaryKey] = change;
        }
    }
}
