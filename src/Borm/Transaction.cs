using Borm.Data;

namespace Borm;

/// <summary>
/// Represents a transactional scope for write operations performed through a <see cref="DataContext"/>.
/// </summary>
///
/// <remarks>
///     <para>
///         If any of the entries affected by this transaction have been changed by another transaction, the operations will be re-executed in the same order.
///     </para>
///     <para>
///         Created by calling <see cref="DataContext.BeginTransaction"/>.
///     </para>
/// </remarks>
public sealed class Transaction : InternalTransaction
{
    private readonly DataContext _context;
    private readonly bool _writeOnCommit;

    internal Transaction(DataContext context, bool writeOnCommit)
        : base(context.TableGraph)
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
