using Borm.Model.Metadata;
using System.Linq.Expressions;

namespace Borm.Model.Conversion;

internal abstract class ConverterFactory<T>
    where T : Delegate
{
    private protected readonly IReadOnlyList<IColumnMetadata> Columns;

    protected ConverterFactory(IReadOnlyList<IColumnMetadata> columns)
    {
        if (!columns.Any())
        {
            throw new ArgumentException("No columns were provided");
        }
        Columns = columns;
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

        UnaryExpression castValue = Expression.Convert(bufValue, column.DataType.RawType);
        if (!column.Constraints.HasFlag(Constraints.AllowDbNull))
        {
            return castValue;
        }

        BinaryExpression isDbNull = Expression.Equal(bufValue, Expression.Constant(DBNull.Value));
        ConstantExpression nullValue = Expression.Constant(null, column.DataType.RawType);

        return Expression.Condition(isDbNull, nullValue, castValue);
    }
}
