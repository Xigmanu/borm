using System.Linq.Expressions;
using System.Reflection;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Model.Validation.Expressions;

// TODO Merge conditional expressions on the same property
internal sealed class ColumnValidationDelegateFactory
{
    private const string CommonEntityVarName = "e";
    private readonly PropertyInfo _cMNameProperty;
    private readonly MethodInfo _eMGetColumnMethod;
    private readonly PropertyInfo _eMNameProperty;
    private readonly Type _entityType;
    private readonly MethodInfo _errorMethod;
    private readonly IReadOnlyList<MappingMember> _properties;

    public ColumnValidationDelegateFactory(Type entityType, IReadOnlyList<MappingMember> properties)
    {
        _entityType = entityType;
        _properties = properties;
        _errorMethod =
            typeof(ValidationResult).GetMethod(
                nameof(ValidationResult.Error),
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy
            )
            ?? throw new InvalidOperationException(
                $"Method {nameof(ValidationResult.Error)} was not found on {nameof(ValidationResult)}"
            );

        Type metaType = typeof(IEntityMetadata);
        _eMNameProperty = metaType.GetProperty(nameof(IEntityMetadata.Name))!;
        _eMGetColumnMethod = metaType.GetMethod(nameof(IEntityMetadata.GetColumn))!;
        _cMNameProperty = typeof(IColumnMetadata).GetProperty(nameof(IColumnMetadata.Name))!;
    }

    public ObjectValidator? Create()
    {
        Type validationResultType = typeof(ValidationResult);
        ParameterExpression boxedEntityParameter = Expression.Parameter(typeof(object), "obj");
        ParameterExpression metadataParam = Expression.Parameter(typeof(IEntityMetadata), "meta");

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
                metadataParam,
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
            .Lambda<ObjectValidator>(
                Expression.Block([unboxedEntityVar, defRetVar], expressions),
                boxedEntityParameter,
                metadataParam
            )
            .Compile();
    }

    private ConditionalExpression CreateIfThenExpression(
        ValidatorExpressionInfo validation,
        string memberName,
        ParameterExpression unboxedEntityVar,
        ParameterExpression metadataParam,
        ParameterExpression defRetVar
    )
    {
        (Expression conditionBody, Expression propAccess) =
            ValidatorExpressionInfo.AdjustToCommonParameter(validation, unboxedEntityVar);

        MemberExpression nameProp = Expression.Property(metadataParam, _eMNameProperty);
        MethodCallExpression getColCall = Expression.Call(
            metadataParam,
            _eMGetColumnMethod,
            Expression.Constant(memberName)
        );
        MemberExpression cName = Expression.Property(getColCall, _cMNameProperty);

        MethodCallExpression errMethodCall = Expression.Call(
            _errorMethod,
            Expression.Convert(propAccess, typeof(object)),
            nameProp,
            cName
        );

        ConditionalExpression ifThen = Expression.IfThen(
            Expression.Not(conditionBody),
            Expression.Assign(defRetVar, errMethodCall)
        );
        return ifThen;
    }
}