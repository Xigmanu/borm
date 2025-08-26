namespace Borm.Data.Sql;

internal sealed class InMemoryCommandExecutor : IDbCommandExecutor
{
    public void ExecuteBatch(DbCommandDefinition command) { }

    public Task ExecuteBatchAsync(DbCommandDefinition command)
    {
        return Task.CompletedTask;
    }

    public ResultSet Query(DbCommandDefinition command)
    {
        return new ResultSet();
    }

    public bool TableExists(string tableName)
    {
        return false;
    }
}
