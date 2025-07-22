using System.Data;
using Borm.Data;

namespace Borm;

public sealed class Transaction : IDisposable, IAsyncDisposable
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);
    private readonly DataContext _context;
    private readonly DataSet _dataSetCopy;
    private readonly bool _writeOnCommit;
    private bool _committed;
    private Exception? _exception;

    internal Transaction(DataContext context, bool writeOnCommit)
    {
        _context = context;
        _writeOnCommit = writeOnCommit;
        _dataSetCopy = context.DataSet.Copy();
        _exception = null;
        _committed = false;
    }

    public Exception? Exception => _exception;

    public void Dispose()
    {
        try
        {
            CommitInternal();
        }
        finally
        {
            _dataSetCopy.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await CommitInternalAsync();
        }
        finally
        {
            _dataSetCopy.Dispose();
        }
    }

    internal void Execute(string tableName, Action<NodeDataTable> tableOperation)
    {
        NodeDataTable tableCopy = GetTableCopy(tableName);
        try
        {
            tableOperation(tableCopy);
        }
        catch (Exception ex)
        {
            _exception ??= ex;
            _dataSetCopy.RejectChanges();
            throw;
        }
    }

    private void CommitInternal()
    {
        if (_exception != null || _committed)
        {
            return;
        }

        _context.DataSet.Merge(_dataSetCopy, preserveChanges: true, MissingSchemaAction.Error);
        if (_writeOnCommit)
        {
            _context.SaveChanges();
        }

        _committed = true;
    }

    private async Task CommitInternalAsync()
    {
        if (_exception != null || _committed || !_dataSetCopy.HasChanges())
        {
            return;
        }

        await Semaphore.WaitAsync();
        try
        {
            _context.DataSet.Merge(_dataSetCopy, preserveChanges: true, MissingSchemaAction.Error);
            
            if (_writeOnCommit)
            {
                await _context.SaveChangesAsync();
            }
        }
        finally
        {
            Semaphore.Release();
        }
    }

    private NodeDataTable GetTableCopy(string tableName)
    {
        return (NodeDataTable)_dataSetCopy.Tables[tableName]!;
    }
}
