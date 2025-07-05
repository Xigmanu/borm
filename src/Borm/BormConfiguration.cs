using System.Data;
using Borm.Data.Sql;

namespace Borm;

public sealed class BormConfiguration
{
    public required Func<IDbConnection> DbConnectionSupplier { get; set; }
    public required ISqlStatementFactory SqlStatementFactory { get; set; }
    public bool TransactionWriteOnCommit { get; set; } = false;
}
