using System.Data.Common;

namespace Borm.Data.Sql;

public interface IDbCommandExecutor
{
    void ExecuteBatch(DbCommandDefinition command);
    Task ExecuteBatchAsync(DbCommandDefinition command);
    DbDataReader ExecuteReader(DbCommandDefinition command);
}
