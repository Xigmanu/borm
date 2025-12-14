using System.Linq.Expressions;
using Borm.Data.Storage;
using Borm.Model.Metadata;

namespace Borm.Model.Conversion.Internal;

internal sealed class PropertyConverterFactory : ConverterFactory<Func<IValueBuffer, object>>
{
    private readonly Type _entityType;

    public PropertyConverterFactory(Type entityType, IReadOnlyList<IColumnMetadata> columns)
        : base(columns)
    {
        _entityType = entityType;
    }

    public override Func<IValueBuffer, object> Create()
    {
        ParameterExpression bufferParam = Expression.Parameter(typeof(IValueBuffer), "buffer");
        ParameterExpression instanceVar = Expression.Variable(_entityType, "instance");

        List<Expression> block = [Expression.Assign(instanceVar, Expression.New(_entityType))];
        block.AddRange(from column in Columns
            let valueExpr = CreateBufferPropertyBinding(bufferParam, column)
            let propertyExpr = Expression.Property(instanceVar, column.PropertyName)
            select Expression.Assign(propertyExpr, valueExpr));
        block.Add(instanceVar);

        return Expression
            .Lambda<Func<IValueBuffer, object>>(Expression.Block([instanceVar], block), bufferParam)
            .Compile();
    }
}