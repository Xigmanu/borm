using System.Diagnostics;

namespace Borm.Data.Storage.Tracking;

internal static class ChangeFactory
{
    public static IChange Delete(IChange existing, IValueBuffer buffer, long writeTxId)
    {
        return ModifyRecord(existing, buffer, writeTxId, RowAction.Delete);
    }

    public static IChange Initial(IValueBuffer buffer, long txId)
    {
        return new Change(buffer, txId, txId, isWrittenToDataSource: true, RowAction.None);
    }

    public static IChange NewChange(IValueBuffer buffer, long txId)
    {
        return new Change(buffer, txId, txId, isWrittenToDataSource: false, RowAction.Insert);
    }

    public static IChange Update(IChange existing, IValueBuffer buffer, long writeTxId)
    {
        return ModifyRecord(existing, buffer, writeTxId, RowAction.Update);
    }

    private static Change ModifyRecord(
        IChange existing,
        IValueBuffer buffer,
        long writeTxId,
        RowAction rowAction
    )
    {
        Debug.Assert(rowAction == RowAction.Update || rowAction == RowAction.Delete);
        return new Change(
            buffer,
            existing.ReadId,
            writeTxId,
            existing.IsWrittenToDataSource,
            rowAction
        );
    }
}
