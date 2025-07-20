namespace Borm.Model;
public interface IEntityValidator<in T>
{
    void Validate(T entity);
}
