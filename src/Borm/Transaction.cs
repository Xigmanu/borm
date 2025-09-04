using Borm.Data;

namespace Borm;

/// <summary>
/// Represents a transactional scope for operations performed through a <see cref="DataContext"/>.
/// Operations executed within this scope are only committed internally if there are no errors. Otherwise, all changes are rolled back. 
/// If any of the entries affected by this transaction have been changed by another transaction, the operations are re-executed in the same order.
/// </summary>
/// 
/// <remarks>
///     Created by calling <see cref="DataContext.BeginTransaction"/>.
/// </remarks>
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
