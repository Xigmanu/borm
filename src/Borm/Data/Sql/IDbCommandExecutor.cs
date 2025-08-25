namespace Borm.Data.Sql;

public interface IDbCommandExecutor
{
    void ExecuteBatch(DbCommandDefinition command);

    Task ExecuteBatchAsync(DbCommandDefinition command);

    ResultSet ReadRows(DbCommandDefinition command);

    // TODO Move this somewhere else for migrations
    bool TableExists(string tableName);
}
