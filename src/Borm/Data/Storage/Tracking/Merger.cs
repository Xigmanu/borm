namespace Borm.Data.Storage.Tracking;

internal static class Merger
{
    public static IChange? CommitMerge(IChange existing, IChange incoming)
    {
        return MergeInternal(existing, incoming, isCommit: true);
    }

    public static IChange? Merge(IChange existing, IChange incoming)
    {
        return MergeInternal(existing, incoming, isCommit: false);
    }

    private static Change? MergeInternal(IChange existing, IChange incoming, bool isCommit)
    {
        // Normally, if the read IDs of both changes are equal,
        // it means that the row was not modified by another transaction while the incoming transaction was open.
        // Here, I attempt to trigger a 'rerun' for the transaction.
        if (existing.ReadId > incoming.ReadId)
        {
            throw new ConcurrencyConflictException("Record was modified by another transaction");
        }

        RowAction rowAction;
        if (existing.IsWrittenToDataSource)
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
            incoming.Record,
            isCommit ? incoming.WriteId : existing.ReadId,
            incoming.WriteId,
            existing.IsWrittenToDataSource,
            rowAction
        );
    }
}
