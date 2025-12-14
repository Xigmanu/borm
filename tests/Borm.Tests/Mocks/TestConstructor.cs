using System.Linq.Expressions;
using Borm.Reflection;

namespace Borm.Tests.Mocks;

internal sealed class TestConstructor : IConstructor
{
    private readonly Func<IEnumerable<Expression>, Expression> _instanceCreator;

    public TestConstructor(
        bool isDefault,
        IReadOnlyList<IMappable> parameters,
        Func<IEnumerable<Expression>, Expression> instanceCreator
    )
    {
        IsDefault = isDefault;
        Parameters = parameters;
        _instanceCreator = instanceCreator;
    }

    public TestConstructor(bool isDefault, IReadOnlyList<IMappable> parameters)
        : this(isDefault, parameters, _ => Expression.Empty())
    {
    }

    public bool IsDefault { get; }
    public IReadOnlyList<IMappable> Parameters { get; }

    public Expression CreateNewInstanceExpression(IEnumerable<Expression> args) =>
        _instanceCreator(args);
}