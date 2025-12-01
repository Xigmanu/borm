using System.Linq.Expressions;

namespace Borm.Reflection;

public interface IConstructor
{
    bool IsDefault { get; }
    IReadOnlyList<IMappable> Parameters { get; }
    Expression CreateNewInstanceExpression(IEnumerable<Expression> args);
}