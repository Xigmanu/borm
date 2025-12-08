using System.Linq.Expressions;
using Borm.Model.Validation.Expressions.Visitors;

namespace Borm.Reflection;

public sealed class ValidationInfo
{
    private readonly Expression _lambda;
    private readonly Expression _propertyAccess;

    internal ValidationInfo(Expression lambda, Expression propertyAccess)
    {
        _lambda = lambda;
        _propertyAccess = propertyAccess;
    }

    internal (Expression body, Expression propAccess) AdjustToCommonParameter(
        ParameterExpression parameter
    )
    {
        LambdaExpression lambda =
            _lambda as LambdaExpression
            ?? throw new InvalidOperationException("Provided expression is not a lambda expression.");
        ParameterExpression oldParameter = lambda.Parameters[0];

        ParameterExpressionRemapper remapper = new(oldParameter, parameter);

        return (body: remapper.Visit(lambda.Body), propAccess: remapper.Visit(_propertyAccess));
    }
}