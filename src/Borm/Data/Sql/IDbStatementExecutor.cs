using System.Data;

namespace Borm.Data.Sql;

public interface IDbStatementExecutor
{
    void ExecuteBatch(SqlStatement statement);

    Task ExecuteBatchAsync(SqlStatement statement);
    IDataReader ExecuteReader(SqlStatement statement);
}
