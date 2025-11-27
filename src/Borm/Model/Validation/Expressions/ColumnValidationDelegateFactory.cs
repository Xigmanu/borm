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
        _errorMethod = typeof(ValidationResult).GetMethod(
            nameof(ValidationResult.Error),
            BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy
        )!;

        Type metaType = typeof(IEntityMetadata);
        _eMNameProperty = metaType.GetProperty(nameof(IEntityMetadata.Name))!;
        _eMGetColumnMethod = metaType.GetMethod(nameof(IEntityMetadata.GetColumn))!;
        _cMNameProperty = typeof(IColumnMetadata).GetProperty(nameof(IColumnMetadata.Name))!;
    }

    public ObjectValidator? Create()
    {
        Type validationResultType = typeof(ValidationResult);
        ParameterExpression boxedEntity = Expression.Parameter(typeof(object), "obj");
        ParameterExpression metadata = Expression.Parameter(typeof(IEntityMetadata), "meta");

        ParameterExpression unboxedEntity = Expression.Variable(_entityType, CommonEntityVarName);
        ParameterExpression ret = Expression.Variable(validationResultType, "result");

        List<Expression> expressions =
        [
            Expression.Assign(unboxedEntity, Expression.Convert(boxedEntity, _entityType)),
            Expression.Assign(
                ret,
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
                unboxedEntity,
                metadata,
                ret
            );
            expressions.Add(ifThen);
        }

        if (expressions.Count == initialCount)
        {
            return null;
        }

        expressions.Add(ret);

        return Expression
            .Lambda<ObjectValidator>(
                Expression.Block([unboxedEntity, ret], expressions),
                boxedEntity,
                metadata
            )
            .Compile();
    }

    private ConditionalExpression CreateIfThenExpression(
        ValidatorExpressionInfo validation,
        string memberName,
        ParameterExpression unboxedEntity,
        ParameterExpression metadata,
        ParameterExpression ret
    )
    {
        (Expression conditionBody, Expression propAccess) =
            ValidatorExpressionInfo.AdjustToCommonParameter(validation, unboxedEntity);

        MemberExpression nameProp = Expression.Property(metadata, _eMNameProperty);
        MethodCallExpression getColCall = Expression.Call(
            metadata,
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
            Expression.Assign(ret, errMethodCall)
        );
        return ifThen;
    }
}