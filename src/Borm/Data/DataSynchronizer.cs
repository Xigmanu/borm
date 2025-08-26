using Borm.Data.Sql;
using Borm.Data.Storage;

namespace Borm.Data;

internal sealed class DataSynchronizer
{
    private readonly IDbCommandDefinitionFactory _commandFactory;
    private readonly IDbCommandExecutor _executor;
    private readonly TableGraph _tableGraph;

    public DataSynchronizer(
        IDbCommandExecutor executor,
        TableGraph tableGraph,
        IDbCommandDefinitionFactory statementFactory
    )
    {
        _executor = executor;
        _tableGraph = tableGraph;
        _commandFactory = statementFactory;
    }

    public void SaveChanges()
    {
        IEnumerable<Table> sorted = _tableGraph.TopSort();
        foreach (Table table in sorted)
        {
            ChangeCommandBuilder commandBuilder = new(table, _commandFactory);
            IEnumerable<DbCommandDefinition> commands = commandBuilder.BuildUpdateCommands();
            foreach (DbCommandDefinition command in commands)
            {
                _executor.ExecuteBatch(command);
            }
            table.Tracker.MarkChangesAsWritten();
        }
    }

    public async Task SaveChangesAsync()
    {
        IEnumerable<Table> sorted = _tableGraph.TopSort();
        foreach (Table table in sorted)
        {
            ChangeCommandBuilder commandBuilder = new(table, _commandFactory);
            IEnumerable<DbCommandDefinition> commands = commandBuilder.BuildUpdateCommands();
            foreach (DbCommandDefinition command in commands)
            {
                await _executor.ExecuteBatchAsync(command);
            }
            table.Tracker.MarkChangesAsWritten();
        }
    }

    public void SyncSchemaWithDataSource()
    {
        using InternalTransaction transaction = new(InternalTransaction.InitId);
        foreach (Table table in _tableGraph.TopSort())
        {
            TableInfo tableSchema = table.GetTableSchema();
            if (!_executor.TableExists(table.Name))
            {
                DbCommandDefinition createTable = _commandFactory.CreateTable(tableSchema);
                _executor.ExecuteBatch(createTable);
                continue;
            }

            DbCommandDefinition selectAll = _commandFactory.SelectAll(tableSchema);
            ResultSet resultSet = _executor.Query(selectAll);

            transaction.Execute(table, (arg, txId) => table.Load((ResultSet)arg, txId), resultSet);
        }
    }
}
