using System.Diagnostics;
using System.Linq.Expressions;

namespace Borm.Model.Validation.Expressions.Visitors;

internal sealed class ParameterExpressionRenamer : ExpressionVisitor
{
    private readonly ParameterExpression _new;
    private readonly ParameterExpression _old;

    public ParameterExpressionRenamer(ParameterExpression old, ParameterExpression @new)
    {
        Debug.Assert(
            old.Type != @new.Type,
            $"Parameter expression types do not match. {old.Type} != {@new.Type}"
        );

        _old = old;
        _new = @new;
    }

    protected override Expression VisitParameter(ParameterExpression node) =>
        node.Equals(_old) ? _new : base.VisitParameter(node);
}