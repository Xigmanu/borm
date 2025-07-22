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
            ((NodeDataTable)table).Load(dataReader);
        }
        dataSet.AcceptChanges();
    }

    public void Update(BormDataSet dataSet)
    {
        EntityNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            DataTable table = dataSet.Tables[sorted[i].Name]!;
            IEnumerable<SqlStatement> statements = CreateUpdateStatements((NodeDataTable)table);
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
            IEnumerable<SqlStatement> statements = CreateUpdateStatements((NodeDataTable)table);
            foreach (SqlStatement statement in statements)
            {
                await _executor.ExecuteBatchAsync(statement);
            }
        }
    }

    private static SqlStatement GetOrCreateSqlStatement(
        DataTable changes,
        DataRow row,
        Dictionary<DataRowState, SqlStatement> rowStateStatements,
        Func<DataTable, SqlStatement> factoryMethod
    )
    {
        if (rowStateStatements.TryGetValue(row.RowState, out SqlStatement? cached))
        {
            return cached;
        }

        SqlStatement statement = factoryMethod(changes);
        rowStateStatements[row.RowState] = statement;
        return statement;
    }

    private Dictionary<DataRowState, SqlStatement>.ValueCollection CreateUpdateStatements(
        NodeDataTable table
    )
    {
        DataTable? changes = table.GetChanges();
        Dictionary<DataRowState, SqlStatement> rowStateStatements = [];
        if (changes == null)
        {
            return rowStateStatements.Values;
        }

        foreach (DataRow row in changes.Rows)
        {
            DataRow? deletedRowClone = null;
            if (row.RowState == DataRowState.Unchanged || row.RowState == DataRowState.Detached)
            {
                continue;
            }

            SqlStatement statement;
            switch (row.RowState)
            {
                case DataRowState.Added:
                    statement = GetOrCreateSqlStatement(
                        changes,
                        row,
                        rowStateStatements,
                        _statementFactory.NewInsertStatement
                    );
                    break;
                case DataRowState.Modified:
                    statement = GetOrCreateSqlStatement(
                        changes,
                        row,
                        rowStateStatements,
                        _statementFactory.NewUpdateStatement
                    );
                    break;
                case DataRowState.Deleted:
                    statement = GetOrCreateSqlStatement(
                        changes,
                        row,
                        rowStateStatements,
                        _statementFactory.NewDeleteStatement
                    );
                    deletedRowClone = ((BormDataSet)table.DataSet!).GetDeletedRowClone(
                        table,
                        changes.Rows.IndexOf(row)
                    );
                    break;
                default:
                    continue;
            }

            Debug.Assert(!string.IsNullOrEmpty(statement.Sql));
            statement.BatchQueue.AddFromRow(deletedRowClone ?? row);
        }

        return rowStateStatements.Values;
    }
}
