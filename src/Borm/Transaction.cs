using Borm.Data;

namespace Borm;

public sealed class Transaction : InternalTransaction, IAsyncDisposable
{
    private readonly bool _writeOnCommit;

    internal Transaction(bool writeOnCommit)
    {
        _writeOnCommit = writeOnCommit;
    }

    protected override void CommitPendingChanges()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
