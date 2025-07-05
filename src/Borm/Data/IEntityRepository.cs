namespace Borm.Data;

public interface IEntityRepository<T> where T : class
{
    bool Delete(T entity);
    bool Delete(T entity, Transaction transaction);
    bool Insert(T entity);
    bool Insert(T entity, Transaction transaction);
    IEnumerable<T> Select();
    IEnumerable<R> Select<R>(Func<T, R> selector);
    bool Update(T entity);
    bool Update(T entity, Transaction transaction);
}
