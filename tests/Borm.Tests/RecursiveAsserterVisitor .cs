using System.Linq.Expressions;

namespace Borm.Tests;

internal sealed class RecursiveAsserterVisitor : ExpressionVisitor
{
    private readonly ParameterExpression _expected;

    public RecursiveAsserterVisitor(ParameterExpression expected)
    {
        _expected = expected;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        Assert.Equal(_expected.Name, node.Name);

        return base.VisitParameter(node);
    }
}