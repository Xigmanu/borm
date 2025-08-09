using Borm.Model.Metadata;

namespace Borm.Data;

internal sealed class Change
{
    private readonly ValueBuffer _buffer;
    private readonly long _txId;
    private bool _isWrittenToDb;
    private RowAction _rowAction;

    public Change(ValueBuffer buffer, long txId, RowAction rowAction, bool isWrittenToDb)
    {
        _txId = txId;
        _isWrittenToDb = isWrittenToDb;
        _rowAction = rowAction;
        _buffer = buffer;
    }

    public Change(ValueBuffer buffer, long txId, RowAction rowAction)
        : this(buffer, txId, rowAction, isWrittenToDb: false) { }

    public bool IsWrittenToDb => _isWrittenToDb;
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

    public void MarkAsWritten()
    {
        _isWrittenToDb = true;
        _rowAction = RowAction.None;
    }

    public Change? Merge(Change incoming)
    {
        if (_txId == incoming._txId)
        {
            return this;
        }
        if (_txId > incoming._txId)
        {
            throw new TransactionIdMismatchException("TODO", _txId, incoming._txId); // TODO this should trigger a rerun for transactions
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
