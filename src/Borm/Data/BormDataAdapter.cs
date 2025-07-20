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
            _executor.ExecuteNonQuery(statement);
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
            List<SqlStatement> statements = CreateUpdateStatements((NodeDataTable)table);
            statements.ForEach(_executor.ExecuteNonQuery);
        }
    }

    private List<SqlStatement> CreateUpdateStatements(NodeDataTable table)
    {
        DataTable? changes = table.GetChanges();
        List<SqlStatement> statements = [];
        if (changes == null)
        {
            return statements;
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
                    statement = _statementFactory.NewInsertStatement(changes);
                    break;
                case DataRowState.Modified:
                    statement = _statementFactory.NewUpdateStatement(changes);
                    break;
                case DataRowState.Deleted:
                    statement = _statementFactory.NewDeleteStatement(changes);
                    deletedRowClone = ((BormDataSet)table.DataSet!).GetDeletedRowClone(
                        table,
                        changes.Rows.IndexOf(row)
                    );
                    break;
                default:
                    continue;
            }

            Debug.Assert(!string.IsNullOrEmpty(statement.Sql));
            statement.SetParameters(deletedRowClone ?? row);

            statements.Add(statement);
        }

        return statements;
    }
}
