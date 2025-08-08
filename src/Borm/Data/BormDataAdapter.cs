using System.Collections.Frozen;
using System.Data;
using System.Diagnostics;
using Borm.Data.Sql;
using Borm.Model.Metadata;

namespace Borm.Data;

internal sealed class BormDataAdapter
{
    private readonly IDbStatementExecutor _executor;
    private readonly EntityNodeGraph _nodeGraph;
    private readonly ISqlStatementFactory _statementFactory;

    public BormDataAdapter(
        IDbStatementExecutor executor,
        EntityNodeGraph nodeGraph,
        ISqlStatementFactory statementFactory
    )
    {
        _executor = executor;
        _nodeGraph = nodeGraph;
        _statementFactory = statementFactory;
    }

    public void CreateTables(IEnumerable<Table> tables)
    {
        EntityNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            Table table = tables.First(table => table.Node == sorted[i]);
            SqlStatement statement = _statementFactory.NewCreateTableStatement(table);
            _executor.ExecuteBatch(statement);
        }
    }

    public void Load(IEnumerable<Table> tables)
    {
        EntityNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            Table table = tables.First(table => table.Node == sorted[i]);
            SqlStatement statement = _statementFactory.NewSelectAllStatement(table);
            using IDataReader dataReader = _executor.ExecuteReader(statement);
            table.Load(dataReader);
        }
    }

    public void Update(IEnumerable<Table> tables)
    {
        EntityNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            Table table = tables.First(table => table.Node == sorted[i]);
            IEnumerable<SqlStatement> statements = CreateUpdateStatements(table);
            foreach (SqlStatement statement in statements)
            {
                _executor.ExecuteBatch(statement);
            }
        }
    }

    public async Task UpdateAsync(IEnumerable<Table> tables)
    {
        EntityNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            Table table = tables.First(table => table.Node == sorted[i]);
            IEnumerable<SqlStatement> statements = CreateUpdateStatements(table);
            foreach (SqlStatement statement in statements)
            {
                await _executor.ExecuteBatchAsync(statement);
            }
        }
    }

    private static SqlStatement GetOrCreateSqlStatement(
        Table table,
        Change entry,
        Dictionary<RowAction, SqlStatement> rowStateStatements,
        Func<Table, SqlStatement> factoryMethod
    )
    {
        if (rowStateStatements.TryGetValue(entry.RowAction, out SqlStatement? cached))
        {
            return cached;
        }

        SqlStatement statement = factoryMethod(table);
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

        foreach (Change entry in changes)
        {
            SqlStatement statement;
            switch (entry.RowAction)
            {
                case RowAction.Insert:
                    statement = GetOrCreateSqlStatement(
                        table,
                        entry,
                        rowStateStatements,
                        _statementFactory.NewInsertStatement
                    );
                    break;
                case RowAction.Update:
                    statement = GetOrCreateSqlStatement(
                        table,
                        entry,
                        rowStateStatements,
                        _statementFactory.NewUpdateStatement
                    );
                    break;
                case RowAction.Delete:
                    statement = GetOrCreateSqlStatement(
                        table,
                        entry,
                        rowStateStatements,
                        _statementFactory.NewDeleteStatement
                    );
                    break;
                default:
                    continue;
            }

            Debug.Assert(!string.IsNullOrEmpty(statement.Sql));
            statement.BatchQueue.AddFromChange(entry);
        }

        return rowStateStatements.Values;
    }
}
