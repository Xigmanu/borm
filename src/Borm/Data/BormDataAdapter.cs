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

    public void CreateTables(BormDataSet dataSet)
    {
        EntityNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            DataTable table = dataSet.Tables[sorted[i].Name]!;
            SqlStatement statement = _statementFactory.NewCreateTableStatement(table);
            _executor.ExecuteBatch(statement);
            table.EndInit();
        }
    }

    public void Load(BormDataSet dataSet)
    {
        EntityNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            DataTable table = dataSet.Tables[sorted[i].Name]!;
            SqlStatement statement = _statementFactory.NewSelectAllStatement(table);
            using IDataReader dataReader = _executor.ExecuteReader(statement);
            ((Table)table).Load(dataReader);
        }
        dataSet.AcceptChanges();
    }

    public void Update(BormDataSet dataSet)
    {
        EntityNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            DataTable table = dataSet.Tables[sorted[i].Name]!;
            IEnumerable<SqlStatement> statements = CreateUpdateStatements((Table)table);
            foreach (SqlStatement statement in statements)
            {
                _executor.ExecuteBatch(statement);
            }
        }
    }

    public async Task UpdateAsync(BormDataSet dataSet)
    {
        EntityNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            DataTable table = dataSet.Tables[sorted[i].Name]!;
            IEnumerable<SqlStatement> statements = CreateUpdateStatements((Table)table);
            foreach (SqlStatement statement in statements)
            {
                await _executor.ExecuteBatchAsync(statement);
            }
        }
    }

    private static SqlStatement GetOrCreateSqlStatement(
        DataTable table,
        Change entry,
        Dictionary<DataRowAction, SqlStatement> rowStateStatements,
        Func<DataTable, SqlStatement> factoryMethod
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

    private Dictionary<DataRowAction, SqlStatement>.ValueCollection CreateUpdateStatements(
        Table table
    )
    {
        IEnumerable<Change> changes = table.GetChanges();
        Dictionary<DataRowAction, SqlStatement> rowStateStatements = [];
        if (!changes.Any())
        {
            return rowStateStatements.Values;
        }

        foreach (Change entry in changes)
        {
            SqlStatement statement;
            switch (entry.RowAction)
            {
                case DataRowAction.Add:
                    statement = GetOrCreateSqlStatement(
                        table,
                        entry,
                        rowStateStatements,
                        _statementFactory.NewInsertStatement
                    );
                    break;
                case DataRowAction.Change:
                    statement = GetOrCreateSqlStatement(
                        table,
                        entry,
                        rowStateStatements,
                        _statementFactory.NewUpdateStatement
                    );
                    break;
                case DataRowAction.Delete:
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
