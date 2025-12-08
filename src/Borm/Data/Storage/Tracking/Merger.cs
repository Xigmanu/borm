namespace Borm.Data.Storage.Tracking;

internal sealed class Merger : IMerger
{
    public IChange? Merge(IChange existing, IChange incoming, MergeMode mode)
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
            mode switch
            {
                MergeMode.Normal => existing.ReadId,
                MergeMode.Commit => incoming.WriteId,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            },
            incoming.WriteId,
            existing.IsWrittenToDataSource,
            rowAction
        );
    }
}