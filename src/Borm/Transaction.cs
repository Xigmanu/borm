using Borm.Data;
using System.Data;

namespace Borm;

public sealed class Transaction : IDisposable
{
    private readonly DataContext _context;
    private readonly DataSet _dataSetCopy;
    private readonly bool _writeOnCommit;
    private Exception? exception;

    public Transaction(DataContext context, bool writeOnCommit)
    {
        _context = context;
        _writeOnCommit = writeOnCommit;
        _dataSetCopy = context.DataSet.Copy();
        exception = null;
    }

    public Exception? Exception => exception;

    public void Dispose()
    {
        TryCommit();
        _dataSetCopy.Dispose();
    }

    internal bool Execute(string tableName, Func<NodeDataTable, bool> tableOp)
    {
        NodeDataTable tableCopy = GetTableCopy(tableName);
        try
        {
            return tableOp(tableCopy);
        }
        catch (Exception ex)
        {
            exception = ex;
            return false;
        }
    }

    private NodeDataTable GetTableCopy(string tableName)
    {
        return (NodeDataTable)_dataSetCopy.Tables[tableName]!;
    }

    private void TryCommit()
    {
        if (exception != null)
        {
            return;
        }

        _context.DataSet.Merge(_dataSetCopy, true, MissingSchemaAction.Error);
        if (_writeOnCommit)
        {
            _context.SaveChanges();
        }
    }
}
