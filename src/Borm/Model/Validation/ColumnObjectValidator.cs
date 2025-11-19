namespace Borm.Model.Validation;

public abstract class ColumnObjectValidator : IObjectValidator<object>
{
    protected ColumnObjectValidator(Type columnDataType)
    {
        ColumnDataType = columnDataType;
    }

    internal Type ColumnDataType { get; }

    public abstract ValidationResult Validate(object value);
}