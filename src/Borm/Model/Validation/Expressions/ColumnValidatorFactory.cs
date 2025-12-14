using System.Linq.Expressions;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Model.Validation.Expressions;

internal sealed class ColumnValidatorFactory
{
    private readonly IValidationExpressionBuilder _builder;
    private readonly ColumnValidatorFactoryContext _context;
    private readonly Type _entityType;
    private readonly IReadOnlyList<IMappable> _properties;

    public ColumnValidatorFactory(
        ColumnValidatorFactoryContext context,
        IValidationExpressionBuilder builder,
        Type entityType,
        IReadOnlyList<IMappable> properties
    )
    {
        _entityType = entityType;
        _properties = properties;
        _context = context;
        _builder = builder;
    }

    public ObjectValidator? Create()
    {
        ParameterExpression boxedEntity = Expression.Parameter(typeof(object), "obj");
        ParameterExpression metadata = Expression.Parameter(typeof(IEntityMetadata), "meta");

        ParameterExpression entity = Expression.Variable(_entityType, "e");
        ParameterExpression result = Expression.Variable(_context.VResultType, "result");

        List<Expression> expressions =
        [
            Expression.Assign(entity, Expression.Convert(boxedEntity, _entityType)),
            Expression.Assign(result, Expression.Call(null, _context.OkMethod))
        ];
        int initialCount = expressions.Count;

        foreach (IMappable property in _properties)
        {
            if (_builder.TryBuild(property, entity, metadata, result, out Expression? validation))
            {
                expressions.Add(validation);
            }
        }

        if (expressions.Count == initialCount)
        {
            return null;
        }

        expressions.Add(result);

        return Expression
            .Lambda<ObjectValidator>(
                Expression.Block([entity, result], expressions),
                boxedEntity,
                metadata
            )
            .Compile();
    }
}