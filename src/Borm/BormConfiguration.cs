using Borm.Data.Sql;
using Borm.Model;

namespace Borm;

public sealed class BormConfiguration
{
    public required IDbCommandExecutor CommandExecutor { get; init; }
    public required EntityModel Model { get; init; }
    public required ISqlCommandDefinitionFactory SqlStatementFactory { get; init; }
    public bool TransactionWriteOnCommit { get; init; } = false;
}
