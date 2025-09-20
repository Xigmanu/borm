using System.Collections;
using System.Diagnostics;
using Borm.Properties;

namespace Borm.Data.Storage;

[DebuggerDisplay("Count = {_changePKMap.Count}")]
internal sealed class ChangeSet : IEnumerable<Change>
{
    private readonly Dictionary<object, Change> _changePKMap;
    private readonly HashSet<object> _danglingKeyCache;

    public ChangeSet()
    {
        (_changePKMap, _danglingKeyCache) = ([], []);
    }

    private ChangeSet(Dictionary<object, Change> changePkMap, HashSet<object> danglingKeyCache)
    {
        _changePKMap = changePkMap;
        _danglingKeyCache = danglingKeyCache;
    }

    public int Count => _changePKMap.Count;

    public static ChangeSet Merge(ChangeSet existing, ChangeSet incoming)
    {
        if (incoming.Count == 0)
        {
            // Assume that all changes have been deleted
            return new ChangeSet(existing._changePKMap, incoming._danglingKeyCache);
        }

        Dictionary<object, Change> resultMap = new(existing._changePKMap);
        foreach ((object incomingPk, Change incomingChange) in incoming._changePKMap)
        {
            if (existing._changePKMap.TryGetValue(incomingPk, out Change? existingChange))
            {
                Change? merged = existingChange.CommitMerge(incomingChange);
                resultMap.Remove(incomingPk);
                if (merged != null)
                {
                    resultMap[incomingPk] = merged;
                }
            }
            else
            {
                if (
                    existing._danglingKeyCache.Contains(incomingPk)
                    || incomingChange.RowAction != RowAction.Insert
                        && incomingChange.WriteTxId != InternalTransaction.InitId
                )
                {
                    throw new InvalidOperationException(Strings.ModificationOfNonExistingRow());
                }
                resultMap[incomingPk] = incomingChange;
            }
        }

        return new ChangeSet(resultMap, [.. incoming._danglingKeyCache]);
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
            else
            {
                _danglingKeyCache.Add(primaryKey);
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
        _danglingKeyCache.Clear();
    }

    public void ReplaceRange(ChangeSet changes)
    {
        _changePKMap.Clear();
        foreach ((object primaryKey, Change change) in changes._changePKMap)
        {
            _changePKMap[primaryKey] = change;
        }
        foreach (object danglingKey in changes._danglingKeyCache)
        {
            _danglingKeyCache.Add(danglingKey);
        }
    }
}
