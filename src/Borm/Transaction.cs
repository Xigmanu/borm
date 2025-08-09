using Borm.Data;

namespace Borm;

public sealed class Transaction : InternalTransaction
{
    private readonly DataContext _context;
    private readonly bool _writeOnCommit;

    internal Transaction(DataContext context, bool writeOnCommit)
    {
        _context = context;
        _writeOnCommit = writeOnCommit;
    }

    protected override void CommitPendingChanges()
    {
        base.CommitPendingChanges();

        if (_writeOnCommit)
        {
            _context.SaveChanges();
        }
    }
}
