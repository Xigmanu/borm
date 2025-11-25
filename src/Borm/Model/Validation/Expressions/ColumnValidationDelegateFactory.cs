using System.Linq.Expressions;
using System.Reflection;
using Borm.Reflection;

namespace Borm.Model.Validation.Expressions;

// TODO Merge conditional expressions on the same property
internal sealed class ColumnValidationDelegateFactory
{
    private const string CommonEntityVarName = "e";
    private readonly Type _entityType;
    private readonly MethodInfo _errorMethod;
    private readonly IReadOnlyList<MappingMember> _properties;

    public ColumnValidationDelegateFactory(Type entityType, IReadOnlyList<MappingMember> properties)
    {
        _entityType = entityType;
        _properties = properties;
        _errorMethod =
            typeof(ValidationResult).GetMethod(nameof(ValidationResult.Error))
            ?? throw new InvalidOperationException(
                $"Method {nameof(ValidationResult.Error)} was not found on {nameof(ValidationResult)}"
            );
    }

    public ValidatorFunc? Create()
    {
        Type validationResultType = typeof(ValidationResult);
        ParameterExpression boxedEntityParameter = Expression.Parameter(typeof(object), "obj");

        ParameterExpression unboxedEntityVar = Expression.Variable(
            _entityType,
            CommonEntityVarName
        );
        ParameterExpression defRetVar = Expression.Variable(validationResultType, "result");

        List<Expression> expressions =
        [
            Expression.Assign(
                unboxedEntityVar,
                Expression.Convert(boxedEntityParameter, _entityType)
            ),
            Expression.Assign(
                defRetVar,
                Expression.Field(null, validationResultType, nameof(ValidationResult.Ok))
            )
        ];
        int initialCount = expressions.Count;

        foreach (MappingMember property in _properties)
        {
            ValidatorExpressionInfo? validation = property.Validation;
            if (validation == null)
            {
                continue;
            }

            ConditionalExpression ifThen = CreateIfThenExpression(
                validation,
                property.MemberName,
                unboxedEntityVar,
                defRetVar
            );
            expressions.Add(ifThen);
        }

        if (expressions.Count == initialCount)
        {
            return null;
        }

        expressions.Add(defRetVar);

        return Expression
            .Lambda<ValidatorFunc>(
                Expression.Block([unboxedEntityVar, defRetVar], expressions),
                boxedEntityParameter
            )
            .Compile();
    }

    private ConditionalExpression CreateIfThenExpression(
        ValidatorExpressionInfo validation,
        string memberName,
        ParameterExpression unboxedEntityVar,
        ParameterExpression defRetVar
    )
    {
        (Expression conditionBody, Expression propAccess) =
            ValidatorExpressionInfo.AdjustToCommonParameter(validation, unboxedEntityVar);

        MethodCallExpression errMethodCall = Expression.Call(
            _errorMethod,
            Expression.Convert(propAccess, typeof(object)),
            Expression.Constant(null, typeof(string)),
            Expression.Constant($"{CommonEntityVarName}.{memberName}")
        );

        ConditionalExpression ifThen = Expression.IfThen(
            Expression.Not(conditionBody),
            Expression.Assign(defRetVar, errMethodCall)
        );
        return ifThen;
    }
}