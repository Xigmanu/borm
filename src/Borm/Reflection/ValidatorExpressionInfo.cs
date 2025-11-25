using System.Linq.Expressions;
using Borm.Model.Validation.Expressions.Visitors;

namespace Borm.Reflection;

// TODO Check if this actually does anything
internal sealed record ValidatorExpressionInfo(
    LambdaExpression ValidationLambda,
    MemberExpression PropertyAccess
)
{
    public static (Expression body, Expression propAccess) AdjustToCommonParameter(ValidatorExpressionInfo info,
        ParameterExpression parameter)
    {
        info.Deconstruct(out LambdaExpression lambda, out MemberExpression propAccess);
        ParameterExpression oldParameter = lambda.Parameters[0];

        ParameterExpressionRenamer renamer = new(oldParameter, parameter);

        return (body: renamer.Visit(lambda.Body), propAccess: renamer.Visit(propAccess));
    }
}