using Borm.Model.Metadata;

namespace Borm.Data;

internal sealed class Change
{
    private readonly ValueBuffer _buffer;
    private readonly RowAction _rowAction;
    private readonly long _txId;

    public Change(ValueBuffer buffer, long txId, RowAction rowAction, bool isWrittenToDb)
    {
        _txId = txId;
        IsWrittenToDb = isWrittenToDb;
        _rowAction = rowAction;
        _buffer = buffer;
    }

    public Change(ValueBuffer buffer, long txId, RowAction rowAction)
        : this(buffer, txId, rowAction, isWrittenToDb: false) { }

    public bool IsWrittenToDb { get; set; }
    public RowAction RowAction => _rowAction;

    internal ValueBuffer Buffer => _buffer;

    public override bool Equals(object? obj)
    {
        return obj is Change other && _buffer.GetPrimaryKey().Equals(other._buffer.GetPrimaryKey());
    }

    public override int GetHashCode()
    {
        return _buffer.GetPrimaryKey().GetHashCode();
    }

    public Change? Merge(Change incoming)
    {
        if (_txId == incoming._txId)
        {
            return this;
        }
        if (_txId > incoming._txId)
        {
            throw new InvalidOperationException(); // TODO this should trigger a rerun for transactions
        }

        RowAction rowAction;
        if (IsWrittenToDb)
        {
            rowAction = incoming._rowAction;
        }
        else
        {
            if (incoming._rowAction == RowAction.Delete)
            {
                return null;
            }
            rowAction = RowAction.Insert;
        }

        return new Change(incoming._buffer, incoming._txId, rowAction, IsWrittenToDb);
    }
}
