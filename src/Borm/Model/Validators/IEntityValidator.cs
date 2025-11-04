namespace Borm.Model.Validators;
public interface IEntityValidator<in T>
{
    void Validate(T entity);
}
