namespace Borm.Model.Validation;

internal interface IConfigurationValidator<in T>
{
    void Validate(T value);
}