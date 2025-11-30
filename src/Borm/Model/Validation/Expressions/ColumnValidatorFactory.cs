using System.Linq.Expressions;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Model.Validation.Expressions;

// TODO Merge conditional expressions on the same property
internal sealed class ColumnValidatorFactory
{
    private readonly ColumnValidatorFactoryContext _context;
    private readonly Type _entityType;
    private readonly IReadOnlyList<MappingMember> _properties;

    public ColumnValidatorFactory(
        ColumnValidatorFactoryContext context,
        Type entityType,
        IReadOnlyList<MappingMember> properties
    )
    {
        _entityType = entityType;
        _properties = properties;
        _context = context;
    }

    public ObjectValidator? Create()
    {
        ParameterExpression boxedEntity = Expression.Parameter(typeof(object), "obj");
        ParameterExpression metadata = Expression.Parameter(typeof(IEntityMetadata), "meta");

        ParameterExpression unboxedEntity = Expression.Variable(_entityType, "e");
        ParameterExpression ret = Expression.Variable(_context.VResultType, "result");

        List<Expression> expressions =
        [
            Expression.Assign(unboxedEntity, Expression.Convert(boxedEntity, _entityType)),
            Expression.Assign(
                ret,
                Expression.Field(null, _context.VResultType, nameof(ValidationResult.Ok))
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

        MemberExpression nameProp = Expression.Property(metadata, _context.EMetaName);
        MethodCallExpression getColCall = Expression.Call(
            metadata,
            _context.EMetaGetColumn,
            Expression.Constant(memberName)
        );
        MemberExpression cName = Expression.Property(getColCall, _context.CMetaName);

        MethodCallExpression errMethodCall = Expression.Call(
            _context.ErrorMethod,
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