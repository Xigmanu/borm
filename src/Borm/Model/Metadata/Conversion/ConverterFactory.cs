using System.Linq.Expressions;

namespace Borm.Model.Metadata.Conversion;

internal abstract class ConverterFactory<T>
    where T : Delegate
{
    private protected readonly IEnumerable<ColumnMetadata> columns;

    protected ConverterFactory(IEnumerable<ColumnMetadata> columns)
    {
        this.columns = columns;
    }

    public abstract T Create();

    protected static Expression CreateBufferPropertyBinding(
        ParameterExpression bufferParam,
        ColumnMetadata column
    )
    {
        IndexExpression bufValue = Expression.Property(
            bufferParam,
            "Item",
            Expression.Constant(column)
        );

        UnaryExpression castValue = Expression.Convert(bufValue, column.PropertyType);
        if (!column.Constraints.HasFlag(Constraints.AllowDbNull))
        {
            return castValue;
        }

        BinaryExpression isDbNull = Expression.Equal(bufValue, Expression.Constant(DBNull.Value));
        ConstantExpression nullValue = Expression.Constant(null, column.PropertyType);

        return Expression.Condition(isDbNull, nullValue, castValue);
    }
}
