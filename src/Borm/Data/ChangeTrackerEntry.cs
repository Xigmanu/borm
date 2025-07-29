using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Borm.Data;

internal readonly struct ChangeTrackerEntry : IEquatable<ChangeTrackerEntry>
{
    private readonly object?[] _data;
    private readonly long _insertTx;
    private readonly bool _isWrittenToDb;
    private readonly DataRowAction _rowAction;

    public ChangeTrackerEntry(
        object?[] data,
        long insertTx,
        bool isWrittenToDb,
        DataRowAction rowAction
    )
    {
        _data = data;
        _insertTx = insertTx;
        _isWrittenToDb = isWrittenToDb;
        _rowAction = rowAction;
    }

    public ChangeTrackerEntry(object?[] data, long insertTx, DataRowAction rowAction)
        : this(data, insertTx, isWrittenToDb: false, rowAction) { }

    public DataRowAction RowAction => _rowAction;

    public static bool operator !=(ChangeTrackerEntry first, ChangeTrackerEntry second)
    {
        return !Equals(first, second);
    }

    public static bool operator ==(ChangeTrackerEntry first, ChangeTrackerEntry second)
    {
        return Equals(first, second);
    }

    public bool Equals(ChangeTrackerEntry other)
    {
        return _insertTx == other._insertTx;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is ChangeTrackerEntry other && Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode() ^ _insertTx.GetHashCode();
    }

    public ChangeTrackerEntry? Merge(ChangeTrackerEntry incoming)
    {
        if (_insertTx == incoming._insertTx)
        {
            return this;
        }
        if (_insertTx > incoming._insertTx)
        {
            throw new InvalidOperationException();
        }

        DataRowAction operation;
        if (_isWrittenToDb)
        {
            operation = incoming._rowAction;
        }
        else
        {
            if (incoming._rowAction == DataRowAction.Delete)
            {
                return null;
            }
            operation = DataRowAction.Add;
        }

        object[] data = new object[_data.Length];
        Array.Copy(incoming._data, data, data.Length);

        return new ChangeTrackerEntry(data, incoming._insertTx, _isWrittenToDb, operation);
    }

    public ChangeTrackerEntry WriteToDb()
    {
        return new ChangeTrackerEntry(_data, _insertTx, isWrittenToDb: true, DataRowAction.Nothing);
    }
}
