using Borm.Data.Storage;

namespace Borm.Data;

internal sealed class EntityRepository<T> : IEntityRepository<T>
    where T : class
{
    private readonly SemaphoreSlim _semaphore;
    private readonly Table _table;

    public EntityRepository(Table table)
    {
        _table = table;
        _semaphore = new(1, 1);
    }

    public void Delete(T entity)
    {
        Execute(entity, _table.Delete);
    }

    public void Delete(T entity, Transaction transaction)
    {
        Execute(entity, _table.Delete, transaction);
    }

    public ValueTask DeleteAsync(T entity)
    {
        return ExecuteAsync(entity, _table.Delete);
    }

    public void Insert(T entity)
    {
        Execute(entity, _table.Insert);
    }

    public void Insert(T entity, Transaction transaction)
    {
        Execute(entity, _table.Insert, transaction);
    }

    public ValueTask InsertAsync(T entity)
    {
        return ExecuteAsync(entity, _table.Insert);
    }

    public IEnumerable<T> Select()
    {
        return _table.SelectAll().Cast<T>();
    }

    public IEnumerable<R> Select<R>(Func<T, R> selector)
    {
        return Select().Select(selector);
    }

    public Task<IEnumerable<T>> SelectAsync()
    {
        return Task.FromResult(Select());
    }

    public Task<IEnumerable<R>> SelectAsync<R>(Func<T, R> selector)
    {
        return Task.FromResult(Select(selector));
    }

    public void Update(T entity)
    {
        Execute(entity, _table.Update);
    }

    public void Update(T entity, Transaction transaction)
    {
        Execute(entity, _table.Update, transaction);
    }

    public ValueTask UpdateAsync(T entity)
    {
        return ExecuteAsync(entity, _table.Update);
    }

    private void Execute(T entity, Action<object, long> operation, InternalTransaction transaction)
    {
        ArgumentNullException.ThrowIfNull(entity);
        transaction.Execute(_table, operation, entity);
    }

    private void Execute(T entity, Action<object, long> operation)
    {
        using InternalTransaction transaction = new();
        Execute(entity, operation, transaction);
    }

    private async ValueTask ExecuteAsync(T entity, Action<object, long> operation)
    {
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            using InternalTransaction transaction = new();
            Execute(entity, operation, transaction);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
