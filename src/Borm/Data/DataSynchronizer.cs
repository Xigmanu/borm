using Borm.Data.Sql;
using Borm.Data.Storage;

namespace Borm.Data;

internal sealed class DataSynchronizer
{
    private readonly CommandBuilder _commandBuilder;
    private readonly IDbCommandDefinitionFactory _commandFactory;
    private readonly IDbCommandExecutor _executor;
    private readonly TableGraph _graph;

    public DataSynchronizer(
        IDbCommandExecutor executor,
        TableGraph graph,
        IDbCommandDefinitionFactory commandFactory
    )
    {
        _executor = executor;
        _graph = graph;
        _commandFactory = commandFactory;
        _commandBuilder = new(graph, commandFactory);
    }

    public void SaveChanges()
    {
        IEnumerable<Table> sorted = _graph.TopSort();
        foreach (Table table in sorted)
        {
            IEnumerable<DbCommandDefinition> commands = _commandBuilder.BuildUpdateCommands(table);
            foreach (DbCommandDefinition command in commands)
            {
                _executor.ExecuteBatch(command);
            }
            table.Tracker.MarkChangesAsWritten();
        }
    }

    public async Task SaveChangesAsync()
    {
        IEnumerable<Table> sorted = _graph.TopSort();
        foreach (Table table in sorted)
        {
            IEnumerable<DbCommandDefinition> commands = _commandBuilder.BuildUpdateCommands(table);
            foreach (DbCommandDefinition command in commands)
            {
                await _executor.ExecuteBatchAsync(command);
            }
            table.Tracker.MarkChangesAsWritten();
        }
    }

    public void SyncSchemaWithDataSource()
    {
        using InternalTransaction transaction = new(InternalTransaction.InitId, _graph);
        foreach (Table table in _graph.TopSort())
        {
            TableInfo tableSchema = _graph.GetTableSchema(table);
            if (!_executor.TableExists(table.Name))
            {
                DbCommandDefinition createTable = _commandFactory.CreateTable(tableSchema);
                _executor.ExecuteBatch(createTable);
                continue;
            }

            DbCommandDefinition selectAll = _commandFactory.SelectAll(tableSchema);
            ResultSet resultSet = _executor.Query(selectAll);

            transaction.Execute(
                (txId, affectedTables) =>
                {
                    table.Load(resultSet, txId);
                    affectedTables.Add(table);
                }
            );
        }
    }
}
