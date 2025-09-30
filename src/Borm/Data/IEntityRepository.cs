namespace Borm.Data;

public interface IEntityRepository<T>
    where T : class
{
    void Delete(T entity);
    void Delete(T entity, Transaction transaction);
    void Insert(T entity);
    void Insert(T entity, Transaction transaction);
    IEnumerable<T> Select();
    IEnumerable<R> Select<R>(Func<T, R> selector);
    void Update(T entity);
    void Update(T entity, Transaction transaction);
}
