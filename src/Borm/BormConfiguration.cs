using Borm.Data.Sql;
using Borm.Model;

namespace Borm;

public sealed class BormConfiguration
{
    public required IDbStatementExecutor CommandExecutor { get; init; }
    public required EntityModel Model { get; init; }
    public required ISqlStatementFactory SqlStatementFactory { get; init; }
    public bool TransactionWriteOnCommit { get; init; } = false;
}
