using System.Data.Common;
using System.Diagnostics;
using Borm.Data.Sql;

namespace Borm.Data;

internal sealed class DataAdapter
{
    private readonly IDbStatementExecutor _executor;
    private readonly ISqlStatementFactory _statementFactory;
    private readonly TableGraph _tableGraph;

    public DataAdapter(
        IDbStatementExecutor executor,
        TableGraph tableGraph,
        ISqlStatementFactory statementFactory
    )
    {
        _executor = executor;
        _tableGraph = tableGraph;
        _statementFactory = statementFactory;
    }

    public void CreateTables()
    {
        IEnumerable<Table> sorted = _tableGraph.TopSort();
        foreach (Table table in sorted)
        {
            TableInfo tableSchema = table.GetTableSchema();
            SqlStatement statement = _statementFactory.NewCreateTableStatement(tableSchema);
            _executor.ExecuteBatch(statement);
        }
    }

    public void Load()
    {
        IEnumerable<Table> sorted = _tableGraph.TopSort();
        using InternalTransaction transaction = new();
        foreach (Table table in sorted)
        {
            TableInfo tableSchema = table.GetTableSchema();
            SqlStatement statement = _statementFactory.NewSelectAllStatement(tableSchema);
            using DbDataReader dataReader = _executor.ExecuteReader(statement);
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
            IEnumerable<SqlStatement> statements = CreateUpdateStatements(table);
            foreach (SqlStatement statement in statements)
            {
                _executor.ExecuteBatch(statement);
            }
            table.MarkChangesAsWritten();
        }
    }

    public async Task UpdateAsync()
    {
        IEnumerable<Table> sorted = _tableGraph.TopSort();
        foreach (Table table in sorted)
        {
            IEnumerable<SqlStatement> statements = CreateUpdateStatements(table);
            foreach (SqlStatement statement in statements)
            {
                await _executor.ExecuteBatchAsync(statement);
            }
        }
    }

    private static SqlStatement GetOrCreateSqlStatement(
        TableInfo tableSchema,
        Change entry,
        Dictionary<RowAction, SqlStatement> rowStateStatements,
        Func<TableInfo, SqlStatement> factoryMethod
    )
    {
        if (rowStateStatements.TryGetValue(entry.RowAction, out SqlStatement? cached))
        {
            return cached;
        }

        SqlStatement statement = factoryMethod(tableSchema);
        rowStateStatements[entry.RowAction] = statement;
        return statement;
    }

    private Dictionary<RowAction, SqlStatement>.ValueCollection CreateUpdateStatements(Table table)
    {
        IEnumerable<Change> changes = table.GetChanges();
        Dictionary<RowAction, SqlStatement> rowStateStatements = [];
        if (!changes.Any())
        {
            return rowStateStatements.Values;
        }

        TableInfo tableSchema = table.GetTableSchema();

        foreach (Change entry in changes)
        {
            SqlStatement statement;
            switch (entry.RowAction)
            {
                case RowAction.Insert:
                    statement = GetOrCreateSqlStatement(
                        tableSchema,
                        entry,
                        rowStateStatements,
                        _statementFactory.NewInsertStatement
                    );
                    break;
                case RowAction.Update:
                    statement = GetOrCreateSqlStatement(
                        tableSchema,
                        entry,
                        rowStateStatements,
                        _statementFactory.NewUpdateStatement
                    );
                    break;
                case RowAction.Delete:
                    statement = GetOrCreateSqlStatement(
                        tableSchema,
                        entry,
                        rowStateStatements,
                        _statementFactory.NewDeleteStatement
                    );
                    break;
                default:
                    continue;
            }

            Debug.Assert(!string.IsNullOrEmpty(statement.Sql));
            statement.BatchQueue.Enqueue(entry.Buffer);
        }

        return rowStateStatements.Values;
    }
}
