using System.Data;
using System.Diagnostics;
using Borm.Data.Sql;
using Borm.Schema;

namespace Borm.Data;

internal sealed class BormDataAdapter
{
    private readonly IDbCommand _dbCommand;
    private readonly TableNodeGraph _nodeGraph;
    private readonly ISqlStatementFactory _statementFactory;

    public BormDataAdapter(
        IDbCommand dbCommand,
        TableNodeGraph nodeGraph,
        ISqlStatementFactory statementFactory
    )
    {
        _dbCommand = dbCommand;
        _nodeGraph = nodeGraph;
        _statementFactory = statementFactory;
    }

    public void CreateTables(DataSet dataSet)
    {
        TableNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            DataTable table = dataSet.Tables[sorted[i].Name]!;
            SqlStatement statement = _statementFactory.NewCreateTableStatement(table);
            _dbCommand.CommandText = statement.Sql;
            _dbCommand.ExecuteNonQuery();
        }
        _dbCommand.CommandText = string.Empty;
    }

    public void Load(DataSet dataSet)
    {
        TableNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            DataTable table = dataSet.Tables[sorted[i].Name]!;
            SqlStatement statement = _statementFactory.NewSelectAllStatement(table);
            _dbCommand.CommandText = statement.Sql;
            using IDataReader dataReader = _dbCommand.ExecuteReader();
            ((NodeDataTable)table).Load(dataReader);
        }
        dataSet.AcceptChanges();
    }

    public void Update(DataSet dataSet)
    {
        TableNode[] sorted = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < sorted.Length; i++)
        {
            DataTable table = dataSet.Tables[sorted[i].Name]!;
            int modifiedRows = UpdateTable(table);
        }
    }

    private int UpdateTable(DataTable table)
    {
        DataTable? changes = table.GetChanges();
        if (changes == null)
        {
            return 0;
        }

        try
        {
            int rowCount = 0;
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
                statement.PrepareCommand(_dbCommand);
                rowCount += _dbCommand.ExecuteNonQuery();
            }

            _dbCommand.Transaction?.Commit();
            return rowCount;
        }
        catch (Exception)
        {
            _dbCommand.Transaction?.Rollback();
            return 0;
        }
    }

}
