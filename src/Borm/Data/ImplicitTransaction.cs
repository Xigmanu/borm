namespace Borm.Data;

internal sealed class ImplicitTransaction : TransactionBase
{
    private readonly Table _table;

    public ImplicitTransaction(Table table)
    {
        _table = table;
    }

    private ImplicitTransaction(ImplicitTransaction original)
        : base(original)
    {
        _table = original._table;
    }

    protected override void CommitPendingChanges()
    {
        if (exception == null)
        {
            _table.AcceptPendingChanges(id);
            return;
        }

        if (attempt >= MaxRetries)
        {
            throw new Exception();
        }

        using ImplicitTransaction retryTx = new(this);
        retryTx.RunQueuedOperations();
    }
}
