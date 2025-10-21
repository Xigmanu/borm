namespace Borm.Data.Storage.Tracking;

internal static class ChangeMerger
{
    public static Change? CommitMerge(Change existing, Change incoming)
    {
        return MergeInternal(existing, incoming, isCommit: true);
    }

    public static Change? Merge(Change existing, Change incoming)
    {
        return MergeInternal(existing, incoming, isCommit: false);
    }

    private static Change? MergeInternal(Change existing, Change incoming, bool isCommit)
    {
        // Normally, if the read IDs of both changes are equal,
        // it means that the row was not modified by another transaction while the incoming transaction was open.
        // Here, I attempt to trigger a 'rerun' for the transaction.
        if (existing.ReadTxId > incoming.ReadTxId)
        {
            throw new ConcurrencyConflictException("Record was modified by another transaction");
        }

        RowAction rowAction;
        if (existing.IsWrittenToDb)
        {
            rowAction = incoming.RowAction;
        }
        else
        {
            if (incoming.RowAction == RowAction.Delete)
            {
                return null;
            }
            rowAction = existing.RowAction;
        }

        return new Change(
            incoming.Buffer,
            isCommit ? incoming.WriteTxId : existing.ReadTxId,
            incoming.WriteTxId,
            existing.IsWrittenToDb,
            rowAction
        );
    }
}
