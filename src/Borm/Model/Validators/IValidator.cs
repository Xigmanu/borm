namespace Borm.Model.Validators;

public interface IValidator<in T>
{
    void Validate(T value);
}