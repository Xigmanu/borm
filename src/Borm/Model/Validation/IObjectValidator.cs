namespace Borm.Model.Validation;

public interface IObjectValidator<in T> where T : class
{
    ValidationResult Validate(T value);
}