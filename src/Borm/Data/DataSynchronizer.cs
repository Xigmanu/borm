using Borm.Data.Sql;
using Borm.Data.Storage;

namespace Borm.Data;

internal sealed class DataSynchronizer
{
    private readonly CommandBuilder _commandBuilder;
    private readonly IDbCommandDefinitionFactory _commandFactory;
    private readonly IDbCommandExecutor _executor;
    private readonly ITableGraph _graph;

    public DataSynchronizer(
        IDbCommandExecutor executor,
        ITableGraph graph,
        IDbCommandDefinitionFactory commandFactory
    )
    {
        _executor = executor;
        _graph = graph;
        _commandFactory = commandFactory;
        _commandBuilder = new CommandBuilder(graph, commandFactory);
    }

    public void SaveChanges()
    {
        IEnumerable<ITable> sorted = _graph.TopSort();
        foreach (ITable table in sorted)
        {
            IEnumerable<DbCommandDefinition> commands = _commandBuilder.BuildUpdateCommands(table);
            foreach (DbCommandDefinition command in commands)
            {
                _executor.ExecuteBatch(command);
            }

            table.Tracker.MarkChangesAsWritten();
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        IEnumerable<ITable> sorted = _graph.TopSort();
        foreach (ITable table in sorted)
        {
            IEnumerable<DbCommandDefinition> commands = _commandBuilder.BuildUpdateCommands(table);
            foreach (DbCommandDefinition command in commands)
            {
                await _executor.ExecuteBatchAsync(command, cancellationToken);
            }

            table.Tracker.MarkChangesAsWritten();
        }
    }

    public void SyncSchemaWithDataSource()
    {
        using Transaction transaction = new(Transaction.InitId, _graph);
        foreach (ITable table in _graph.TopSort())
        {
            TableInfo tableSchema = _graph.GetSchema(table);
            DbCommandDefinition selectAll = _commandFactory.SelectAll(tableSchema);
            ResultSet resultSet = _executor.Query(selectAll);

            transaction.Execute((txId, affectedTables) =>
                {
                    table.Load(resultSet, txId);
                    affectedTables.Add(table);
                }
            );
        }
    }
}