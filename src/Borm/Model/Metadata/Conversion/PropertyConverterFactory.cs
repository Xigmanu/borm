using System.Linq.Expressions;
using Borm.Data.Storage;

namespace Borm.Model.Metadata.Conversion;

internal sealed class PropertyConverterFactory : ConverterFactory<Func<IValueBuffer, object>>
{
    private readonly Type _entityType;

    public PropertyConverterFactory(Type entityType, IEnumerable<IColumnMetadata> columns)
        : base(columns)
    {
        _entityType = entityType;
    }

    public override Func<IValueBuffer, object> Create()
    {
        ParameterExpression bufferParam = Expression.Parameter(typeof(IValueBuffer), "buffer");
        ParameterExpression instanceVar = Expression.Variable(_entityType, "instance");

        List<Expression> block = [Expression.Assign(instanceVar, Expression.New(_entityType))];
        foreach (IColumnMetadata column in columns)
        {
            Expression valueExpr = CreateBufferPropertyBinding(bufferParam, column);
            MemberExpression propertyExpr = Expression.Property(instanceVar, column.PropertyName);
            block.Add(Expression.Assign(propertyExpr, valueExpr));
        }
        block.Add(instanceVar);

        return Expression
            .Lambda<Func<IValueBuffer, object>>(Expression.Block([instanceVar], block), bufferParam)
            .Compile();
    }
}
