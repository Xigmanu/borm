using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Borm.Reflection;

namespace Borm.Model.Validation;

// TODO Merge conditional expressions on the same property
// TODO Use common delegate for validations
internal static class ColumnValidationDelegateFactory
{
    private const string CommonEntityVarName = "eObj";

    public static Func<object, ValidationResult> Create(
        Type entityType,
        IReadOnlyList<MappingMember> properties
    )
    {
        ParameterExpression boxedEntityParameter = Expression.Parameter(typeof(object), "obj");

        ParameterExpression unboxedEntityVar = Expression.Variable(
            entityType,
            CommonEntityVarName
        );
        ParameterExpression defRetVar = Expression.Variable(typeof(ValidationResult), "ok");

        MethodInfo? errorMethod = typeof(ValidationResult).GetMethod(
            nameof(ValidationResult.Error)
        );
        Debug.Assert(
            errorMethod != null,
            $"Static method 'Error' was not found in type {nameof(ValidationResult)}"
        );

        List<Expression> block =
        [
            Expression.Assign(unboxedEntityVar, Expression.Convert(boxedEntityParameter, entityType)),
            Expression.Assign(defRetVar, Expression.Field(null, typeof(ValidationResult), nameof(ValidationResult.Ok)))
        ];
        foreach (MappingMember property in properties)
        {
            ValidatorExpressionInfo? validation = property.Validation;
            if (validation == null)
            {
                continue;
            }

            (Expression adjustedBody, Expression adjustedPropAccess) =
                ValidatorExpressionInfo.AdjustToCommonParameter(validation, unboxedEntityVar);

            MethodCallExpression errMethodCall = Expression.Call(
                errorMethod,
                adjustedPropAccess,
                Expression.Constant(null, typeof(string)),
                Expression.Constant($"{CommonEntityVarName}.{property.MemberName}")
            );
            UnaryExpression negated = Expression.Not(adjustedBody);

            block.Add(Expression.IfThen(negated, Expression.Assign(defRetVar, errMethodCall)));
        }

        block.Add(defRetVar);

        return Expression.Lambda<Func<object, ValidationResult>>(Expression.Block([unboxedEntityVar, defRetVar], block),
                boxedEntityParameter)
            .Compile();
    }
}