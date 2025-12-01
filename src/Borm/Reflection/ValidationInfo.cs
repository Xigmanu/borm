using System.Linq.Expressions;
using Borm.Model.Validation.Expressions.Visitors;

namespace Borm.Reflection;

internal sealed class ValidationInfo
{
    private readonly LambdaExpression _lambda;
    private readonly MemberExpression _propertyAccess;

    public ValidationInfo(LambdaExpression lambda, MemberExpression propertyAccess)
    {
        _lambda = lambda;
        _propertyAccess = propertyAccess;
    }

    public (Expression body, Expression propAccess) AdjustToCommonParameter(
        ParameterExpression parameter
    )
    {
        ParameterExpression oldParameter = _lambda.Parameters[0];

        ParameterExpressionRemapper remapper = new(oldParameter, parameter);

        return (body: remapper.Visit(_lambda.Body), propAccess: remapper.Visit(_propertyAccess));
    }
}