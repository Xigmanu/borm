using System.Data.Common;
using System.Diagnostics;
using Borm.Data.Sql;

namespace Borm.Data;

internal sealed class DataAdapter
{
    private readonly IDbCommandExecutor _executor;
    private readonly ISqlCommandDefinitionFactory _commandFactory;
    private readonly TableGraph _tableGraph;

    public DataAdapter(
        IDbCommandExecutor executor,
        TableGraph tableGraph,
        ISqlCommandDefinitionFactory statementFactory
    )
    {
        _executor = executor;
        _tableGraph = tableGraph;
        _commandFactory = statementFactory;
    }

    public void CreateTables()
    {
        IEnumerable<Table> sorted = _tableGraph.TopSort();
        foreach (Table table in sorted)
        {
            TableInfo tableSchema = table.GetTableSchema();
            DbCommandDefinition command = _commandFactory.CreateTable(tableSchema);
            _executor.ExecuteBatch(command);
        }
    }

    public void Load()
    {
        IEnumerable<Table> sorted = _tableGraph.TopSort();
        using InternalTransaction transaction = new();
        foreach (Table table in sorted)
        {
            TableInfo tableSchema = table.GetTableSchema();
            DbCommandDefinition command = _commandFactory.SelectAll(tableSchema);
            using DbDataReader dataReader = _executor.ExecuteReader(command);
            transaction.Execute(
                table,
                (arg, txId) => table.Load((DbDataReader)arg, txId),
                dataReader
            );
        }
    }

    public void Update()
    {
        IEnumerable<Table> sorted = _tableGraph.TopSort();
        foreach (Table table in sorted)
        {
            IEnumerable<DbCommandDefinition> commands = CreateUpdateStatements(table);
            foreach (DbCommandDefinition command in commands)
            {
                _executor.ExecuteBatch(command);
            }
            table.Tracker.MarkChangesAsWritten();
        }
    }

    public async Task UpdateAsync()
    {
        IEnumerable<Table> sorted = _tableGraph.TopSort();
        foreach (Table table in sorted)
        {
            IEnumerable<DbCommandDefinition> commands = CreateUpdateStatements(table);
            foreach (DbCommandDefinition command in commands)
            {
                await _executor.ExecuteBatchAsync(command);
            }
        }
    }

    private static DbCommandDefinition GetOrCreateSqlStatement(
        TableInfo tableSchema,
        Change entry,
        Dictionary<RowAction, DbCommandDefinition> rowStateStatements,
        Func<TableInfo, DbCommandDefinition> factoryMethod
    )
    {
        if (rowStateStatements.TryGetValue(entry.RowAction, out DbCommandDefinition? cached))
        {
            return cached;
        }

        DbCommandDefinition command = factoryMethod(tableSchema);
        rowStateStatements[entry.RowAction] = command;
        return command;
    }

    private Dictionary<RowAction, DbCommandDefinition>.ValueCollection CreateUpdateStatements(
        Table table
    )
    {
        IEnumerable<Change> changes = table.Tracker.Changes;
        Dictionary<RowAction, DbCommandDefinition> rowStateStatements = [];
        if (!changes.Any())
        {
            return rowStateStatements.Values;
        }

        TableInfo tableSchema = table.GetTableSchema();

        foreach (Change entry in changes)
        {
            DbCommandDefinition command;
            switch (entry.RowAction)
            {
                case RowAction.Insert:
                    command = GetOrCreateSqlStatement(
                        tableSchema,
                        entry,
                        rowStateStatements,
                        _commandFactory.Insert
                    );
                    break;
                case RowAction.Update:
                    command = GetOrCreateSqlStatement(
                        tableSchema,
                        entry,
                        rowStateStatements,
                        _commandFactory.Update
                    );
                    break;
                case RowAction.Delete:
                    command = GetOrCreateSqlStatement(
                        tableSchema,
                        entry,
                        rowStateStatements,
                        _commandFactory.Delete
                    );
                    break;
                default:
                    continue;
            }

            Debug.Assert(!string.IsNullOrEmpty(command.Sql));
            command.BatchQueue.Enqueue(entry.Buffer);
        }

        return rowStateStatements.Values;
    }
}
