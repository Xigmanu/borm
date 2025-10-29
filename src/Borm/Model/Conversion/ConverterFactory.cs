using Borm.Model.Metadata;
using System.Linq.Expressions;

namespace Borm.Model.Conversion;

internal abstract class ConverterFactory<T>
    where T : Delegate
{
    private protected readonly IEnumerable<IColumnMetadata> columns;

    protected ConverterFactory(IEnumerable<IColumnMetadata> columns)
    {
        this.columns = columns;
    }

    public abstract T Create();

    protected static Expression CreateBufferPropertyBinding(
        ParameterExpression bufferParam,
        IColumnMetadata column
    )
    {
        IndexExpression bufValue = Expression.Property(
            bufferParam,
            "Item",
            Expression.Constant(column)
        );

        UnaryExpression castValue = Expression.Convert(bufValue, column.DataType.Type);
        if (!column.Constraints.HasFlag(Constraints.AllowDbNull))
        {
            return castValue;
        }

        BinaryExpression isDbNull = Expression.Equal(bufValue, Expression.Constant(DBNull.Value));
        ConstantExpression nullValue = Expression.Constant(null, column.DataType.Type);

        return Expression.Condition(isDbNull, nullValue, castValue);
    }
}
