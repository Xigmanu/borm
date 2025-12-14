using System.Diagnostics;

namespace Borm.Data.Storage.Tracking;

internal static class ChangeFactory
{
    public static IChange Delete(IChange existing, IValueBuffer buffer, long writeTxId)
    {
        return ModifyRecord(existing, buffer, writeTxId, OperationKind.Delete);
    }

    public static IChange Initial(IValueBuffer buffer, long txId)
    {
        return new Change(buffer, txId, txId, true, OperationKind.None);
    }

    public static IChange NewChange(IValueBuffer buffer, long txId)
    {
        return new Change(buffer, txId, txId, false, OperationKind.Insert);
    }

    public static IChange Update(IChange existing, IValueBuffer buffer, long writeTxId)
    {
        return ModifyRecord(existing, buffer, writeTxId, OperationKind.Update);
    }

    private static Change ModifyRecord(
        IChange existing,
        IValueBuffer buffer,
        long writeTxId,
        OperationKind operation
    )
    {
        Debug.Assert(operation == OperationKind.Update || operation == OperationKind.Delete);
        return new Change(
            buffer,
            existing.ReadId,
            writeTxId,
            existing.IsWrittenToDataSource,
            operation
        );
    }
}