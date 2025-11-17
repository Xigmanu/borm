namespace Borm.Model.Validation;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ValidatorAttribute : Attribute
{
    public ValidatorAttribute(Type validatorType)
    {
        ValidatorType = validatorType;
    }

    public Type ValidatorType { get; }
}