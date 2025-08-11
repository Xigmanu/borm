using System.Data.Common;

namespace Borm.Data.Sql;

public interface IDbStatementExecutor
{
    void ExecuteBatch(SqlStatement statement);

    Task ExecuteBatchAsync(SqlStatement statement);
    DbDataReader ExecuteReader(SqlStatement statement);
}
