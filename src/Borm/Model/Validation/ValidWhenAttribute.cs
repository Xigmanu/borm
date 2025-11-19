namespace Borm.Model.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class ValidWhenAttribute : Attribute
{
    public ValidWhenAttribute(ColumnObjectValidator validator)
    {
        
    }
}
