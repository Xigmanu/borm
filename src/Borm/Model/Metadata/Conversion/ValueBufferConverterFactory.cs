using System.Linq.Expressions;
using Borm.Data.Storage;

namespace Borm.Model.Metadata.Conversion;

internal class ValueBufferConverterFactory : ConverterFactory<Func<object, IValueBuffer>>
{
    private readonly Type _entityType;

    public ValueBufferConverterFactory(Type entityType, IEnumerable<IColumnMetadata> columns)
        : base(columns)
    {
        _entityType = entityType;
    }

    public override Func<object, IValueBuffer> Create()
    {
        Type valueBufferType = typeof(ValueBuffer);

        ParameterExpression boxedEntityParam = Expression.Parameter(typeof(object), "entity");
        ParameterExpression unboxedEntityVar = Expression.Variable(_entityType, "entityVar");
        BinaryExpression unbox = Expression.Assign(
            unboxedEntityVar,
            Expression.Convert(boxedEntityParam, _entityType)
        );

        ParameterExpression valueBufferVar = Expression.Variable(valueBufferType, "buffer");

        List<Expression> block =
        [
            unbox,
            Expression.Assign(valueBufferVar, Expression.New(valueBufferType)),
        ];

        foreach (IColumnMetadata column in columns)
        {
            BinaryExpression conditionalAssign = CreateBlockExpression(
                unboxedEntityVar,
                valueBufferVar,
                column
            );
            block.Add(conditionalAssign);
        }

        block.Add(valueBufferVar);

        return Expression
            .Lambda<Func<object, IValueBuffer>>(
                Expression.Block([unboxedEntityVar, valueBufferVar], block),
                boxedEntityParam
            )
            .Compile();
    }

    private static BinaryExpression CreateBlockExpression(
        ParameterExpression unboxedEntityVar,
        ParameterExpression valueBufferVar,
        IColumnMetadata column
    )
    {
        ConstantExpression key = Expression.Constant(column);
        MemberExpression value = Expression.Property(unboxedEntityVar, column.PropertyName);
        UnaryExpression boxedValue = Expression.Convert(value, typeof(object));

        BinaryExpression isNullCheck = Expression.Equal(
            boxedValue,
            Expression.Constant(null, typeof(object))
        );

        IndexExpression indexer = Expression.MakeIndex(
            valueBufferVar,
            typeof(IValueBuffer).GetProperty("Item", [typeof(ColumnMetadata)]),
            [key]
        );

        BinaryExpression conditionalAssign = Expression.Assign(
            indexer,
            Expression.Condition(
                isNullCheck,
                Expression.Constant(DBNull.Value, typeof(object)),
                boxedValue
            )
        );
        return conditionalAssign;
    }
}
