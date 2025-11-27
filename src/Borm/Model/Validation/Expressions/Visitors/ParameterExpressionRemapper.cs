using System.Linq.Expressions;

namespace Borm.Model.Validation.Expressions.Visitors;

internal sealed class ParameterExpressionRemapper : ExpressionVisitor
{
    private readonly Expression _new;
    private readonly ParameterExpression _old;

    public ParameterExpressionRemapper(ParameterExpression old, Expression @new)
    {
        _old = old;
        _new = @new;
    }

    protected override Expression VisitParameter(ParameterExpression node) =>
        node.Name == _old.Name ? _new : base.VisitParameter(node);

    protected override Expression VisitUnary(UnaryExpression node) =>
        node is { NodeType: ExpressionType.Convert, Operand: ParameterExpression inner } && inner.Name == _old.Name
            ? _new
            : base.VisitUnary(node);
}