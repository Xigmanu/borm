namespace Borm.Data;

public interface IEntityRepository<T>
    where T : class
{
    void Delete(T entity);
    void Delete(T entity, Transaction transaction);
    Task DeleteAsync(T entity);
    void Insert(T entity);
    void Insert(T entity, Transaction transaction);
    Task InsertAsync(T entity);
    IEnumerable<T> Select();
    IEnumerable<R> Select<R>(Func<T, R> selector);
    Task<IEnumerable<T>> SelectAsync();
    Task<IEnumerable<R>> SelectAsync<R>(Func<T, R> selector);
    void Update(T entity);
    void Update(T entity, Transaction transaction);
    Task UpdateAsync(T entity);
}
