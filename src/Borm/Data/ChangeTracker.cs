using System.Data;
using System.Diagnostics;

namespace Borm.Data;

internal sealed class ChangeTracker
{
    private readonly Dictionary<object, ChangeTrackerEntry> _changes;

    public ChangeTracker()
    {
        _changes = [];
    }
    // TODO!!!
    // 2. Rework transaction logic: add implicit (internal) and explicit (public) transactions for easier row insert tx id tracking
    public IEnumerable<ChangeTrackerEntry> GetChanges()
    {
        return _changes.Values.Where(change => change.RowAction != DataRowAction.Nothing);
    }

    public void InitInsert(object primaryKey, VersionedDataRow row)
    {
        if (_changes.ContainsKey(primaryKey))
        {
            throw new InvalidOperationException();
        }

        // Debug.Assert(row.InsertTx == 0);
        ChangeTrackerEntry entry = new(row.ItemArray, row.InsertTx, DataRowAction.Nothing);
        _changes[primaryKey] = entry;
    }

    public void Update(object primaryKey, VersionedDataRow row, DataRowAction rowAction)
    {
        ChangeTrackerEntry incoming = new(row.ItemArray, row.InsertTx, rowAction);
        if (_changes.TryGetValue(primaryKey, out ChangeTrackerEntry current))
        {
            ChangeTrackerEntry? merged = current.Merge(incoming);
            if (!merged.HasValue)
            {
                _changes.Remove(primaryKey);
            }
            else
            {
                _changes[primaryKey] = merged.Value;
            }
            return;
        }

        Debug.Assert(rowAction == DataRowAction.Add);
        _changes[primaryKey] = incoming;
    }
}
