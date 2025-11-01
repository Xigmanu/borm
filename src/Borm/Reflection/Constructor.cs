using System.Linq.Expressions;

namespace Borm.Reflection;

public sealed class Constructor
{
    private readonly Func<IEnumerable<Expression>, Expression> _instanceInitializer;

    public Constructor(
        bool isDefault,
        IReadOnlyList<MappingMember> parameters,
        Func<IEnumerable<Expression>, Expression> instanceInitializer
    )
    {
        _instanceInitializer = instanceInitializer;
        IsDefault = isDefault;
        Parameters = parameters;
    }

    public bool IsDefault { get; }
    public IReadOnlyList<MappingMember> Parameters { get; }

    public Expression CreateNewInstanceExpression(IEnumerable<Expression> args) =>
        _instanceInitializer(args);
}
