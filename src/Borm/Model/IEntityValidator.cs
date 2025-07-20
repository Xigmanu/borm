namespace Borm.Schema;
public interface IEntityValidator<in T>
{
    void Validate(T entity);
}
