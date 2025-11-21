using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Borm.Model.Validation.Expressions;

internal sealed class ParameterExpressionRenamer : ExpressionVisitor
{
    private readonly ParameterExpression _new;
    private readonly ParameterExpression _old;

    public ParameterExpressionRenamer(ParameterExpression old, ParameterExpression @new)
    {
        if (old.Type != @new.Type)
        {
            throw new ArgumentException(
                $"Parameter expression types do not match. {old.Type} != {@new.Type}"
            );
        }

        _old = old;
        _new = @new;
    }

    [return: NotNullIfNotNull("node")]
    public override Expression? Visit(Expression? node) =>
        node is ParameterExpression param && param.Equals(_old) ? _new : base.Visit(node);
}