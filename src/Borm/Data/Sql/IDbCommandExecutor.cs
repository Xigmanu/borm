namespace Borm.Data.Sql;

public interface IDbCommandExecutor
{
    void ExecuteBatch(DbCommandDefinition command);

    Task ExecuteBatchAsync(DbCommandDefinition command, CancellationToken cancellationToken);

    ResultSet Query(DbCommandDefinition command);
}