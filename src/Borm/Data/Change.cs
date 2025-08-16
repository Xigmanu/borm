using System.Diagnostics;

namespace Borm.Data;

internal sealed class Change
{
    private readonly ValueBuffer _buffer;
    private readonly long _readTxId;
    private readonly long _writeTxId;
    private bool _isWrittenToDb;
    private RowAction _rowAction;

    private Change(
        ValueBuffer buffer,
        long readTxId,
        long writeTxId,
        bool isWrittenToDb,
        RowAction rowAction
    )
    {
        _buffer = buffer;
        _readTxId = readTxId;
        _writeTxId = writeTxId;
        _isWrittenToDb = isWrittenToDb;
        _rowAction = rowAction;
    }

    public bool IsWrittenToDb => _isWrittenToDb;
    public long ReadTxId => _readTxId;
    public RowAction RowAction => _rowAction;
    public long WriteTxId => _writeTxId;
    internal ValueBuffer Buffer => _buffer;

    public static Change InitChange(ValueBuffer buffer, long txId)
    {
        return new Change(buffer, txId, txId, isWrittenToDb: true, RowAction.None);
    }

    public static Change NewChange(ValueBuffer buffer, long txId)
    {
        return new Change(buffer, txId, txId, isWrittenToDb: false, RowAction.Insert);
    }

    public Change Delete(ValueBuffer buffer, long writeTxId)
    {
        return new Change(buffer, _readTxId, writeTxId, _isWrittenToDb, RowAction.Delete);
    }

    public override bool Equals(object? obj)
    {
        return obj is Change other && _buffer.PrimaryKey.Equals(other._buffer.PrimaryKey);
    }

    public override int GetHashCode()
    {
        return _buffer.PrimaryKey.GetHashCode();
    }

    public void MarkAsWritten()
    {
        _isWrittenToDb = true;
        _rowAction = RowAction.None;
    }

    public Change? Merge(Change incoming, bool isCommit)
    {
        // No change has been made to a row => ignore the incoming
        if (_writeTxId == incoming._writeTxId)
        {
            return this;
        }

        // Normally, if the read IDs of both changes are equal,
        // it means that the row was not modified by another transaction while the incoming transaction was open.
        // Here, I attempt to trigger a 'rerun' for the transaction.
        if (_readTxId > incoming._readTxId)
        {
            throw new TransactionIdMismatchException(
                "Row was modified by another transaction",
                _readTxId,
                incoming._readTxId
            );
        }

        RowAction rowAction;
        if (IsWrittenToDb)
        {
            rowAction = incoming._rowAction;
        }
        else
        {
            // If a row is added and then deleted within the same transaction scope, no actual changes have been made.
            // Remove the dangling entry.
            if (incoming._rowAction == RowAction.Delete)
            {
                return null;
            }

            Debug.Assert(incoming._rowAction == RowAction.Insert);
            rowAction = RowAction.Insert;
        }

        return new Change(
            incoming._buffer,
            isCommit ? incoming._writeTxId : _readTxId,
            incoming._writeTxId,
            IsWrittenToDb,
            rowAction
        );
    }

    public Change Update(ValueBuffer buffer, long writeTxId)
    {
        return new Change(buffer, _readTxId, writeTxId, _isWrittenToDb, RowAction.Update);
    }
}
