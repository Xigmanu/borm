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
        ArgumentNullException.ThrowIfNull(entity);

        using ImplicitTransaction transaction = new(_table);
        transaction.Execute(_table.Delete, entity);
    }

    public void Delete(T entity, Transaction transaction)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(T entity)
    {
        return ExecuteInLock(() => Delete(entity));
    }

    public void Insert(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using ImplicitTransaction transaction = new(_table);
        transaction.Execute(_table.Insert, entity);
    }

    public void Insert(T entity, Transaction transaction)
    {
        throw new NotImplementedException();
    }

    public Task InsertAsync(T entity)
    {
        return ExecuteInLock(() => Insert(entity));
    }

    public IEnumerable<T> Select()
    {
        return _table.Select().Cast<T>();
    }

    public IEnumerable<R> Select<R>(Func<T, R> selector)
    {
        return Select().Select(selector);
    }

    public Task<IEnumerable<T>> SelectAsync()
    {
        return Task.Run(Select);
    }

    public Task<IEnumerable<R>> SelectAsync<R>(Func<T, R> selector)
    {
        return Task.Run(() => Select(selector));
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using ImplicitTransaction transaction = new(_table);
        transaction.Execute(_table.Update, entity);
    }

    public void Update(T entity, Transaction transaction)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(T entity)
    {
        return ExecuteInLock(() => Update(entity));
    }

    private async Task ExecuteInLock(Action action)
    {
        await _semaphore.WaitAsync();
        try
        {
            action();
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
