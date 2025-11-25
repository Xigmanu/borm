using System.Linq.Expressions;

namespace Borm.Model.Validation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class StringLengthAttribute : ValidWhenAttribute
{
    private readonly int _min;
    private readonly int _max;

    public StringLengthAttribute(int min, int max)
    {
        _min = min;
        _max = max;
    }

    protected override Expression<Func<object, bool>> ValidWhen =>
        o => _min <= ((string)o).Length && ((string)o).Length <= _max;
}
