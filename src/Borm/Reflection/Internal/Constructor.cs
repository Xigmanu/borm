using System.Linq.Expressions;

namespace Borm.Reflection.Internal;

public sealed class Constructor : IConstructor
{
    private readonly Func<IEnumerable<Expression>, Expression> _instanceInitializer;

    internal Constructor(
        bool isDefault,
        IReadOnlyList<IMappable> parameters,
        Func<IEnumerable<Expression>, Expression> instanceInitializer
    )
    {
        _instanceInitializer = instanceInitializer;
        IsDefault = isDefault;
        Parameters = parameters;
    }

    public bool IsDefault { get; }
    public IReadOnlyList<IMappable> Parameters { get; }

    public Expression CreateNewInstanceExpression(IEnumerable<Expression> args) =>
        _instanceInitializer(args);
}