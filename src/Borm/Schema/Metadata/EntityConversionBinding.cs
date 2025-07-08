using System.Linq.Expressions;

namespace Borm.Schema.Metadata;

internal sealed class EntityConversionBinding
{
    private readonly Func<object, ValueBuffer> _convertToValueBuffer;
    private readonly Func<ValueBuffer, object> _materializeEntity;

    private EntityConversionBinding(
        Func<ValueBuffer, object> materializeEntity,
        Func<object, ValueBuffer> convertToValueBuffer
    )
    {
        _materializeEntity = materializeEntity;
        _convertToValueBuffer = convertToValueBuffer;
    }

    public static EntityConversionBinding CreateConstructorBased(EntityBindingInfo bindingInfo)
    {
        Func<ValueBuffer, object> materializer = CreateConstructorMaterializer(bindingInfo);
        Func<object, ValueBuffer> converter = CreateEntityValueBufferConverter(bindingInfo);
        return new EntityConversionBinding(materializer, converter);
    }

    public static EntityConversionBinding CreatePropertyBased(EntityBindingInfo bindingInfo)
    {
        Func<ValueBuffer, object> materializer = CreatePropertyMaterializer(bindingInfo);
        Func<object, ValueBuffer> converter = CreateEntityValueBufferConverter(bindingInfo);
        return new EntityConversionBinding(materializer, converter);
    }

    public ValueBuffer Convert(object entity)
    {
        return _convertToValueBuffer(entity);
    }

    public object Materialize(ValueBuffer buffer)
    {
        return _materializeEntity(buffer);
    }

    private static UnaryExpression CreateBufferPropertyBinding(
        ParameterExpression bufferParam,
        ColumnInfo column
    )
    {
        IndexExpression boxedRowValue = Expression.Property(
            bufferParam,
            "Item",
            Expression.Constant(column)
        );
        return Expression.Convert(boxedRowValue, column.DataType);
    }

    private static Func<ValueBuffer, object> CreateConstructorMaterializer(
        EntityBindingInfo bindingInfo
    )
    {
        ParameterExpression bufferParam = Expression.Parameter(typeof(ValueBuffer), "buffer");

        IEnumerable<UnaryExpression> args = bindingInfo
            .GetOrderedColumns()
            .Select(columnInfo => CreateBufferPropertyBinding(bufferParam, columnInfo));
        NewExpression ctorCall = Expression.New(bindingInfo.Constructor, args);

        return Expression
            .Lambda<Func<ValueBuffer, object>>(
                Expression.Convert(ctorCall, typeof(object)),
                bufferParam
            )
            .Compile();
    }

    private static Func<object, ValueBuffer> CreateEntityValueBufferConverter(
        EntityBindingInfo bindingInfo
    )
    {
        Type valueBufferType = typeof(ValueBuffer);

        ParameterExpression boxedEntityParam = Expression.Parameter(typeof(object), "entity");
        ParameterExpression unboxedEntityVar = Expression.Variable(
            bindingInfo.EntityType,
            "typedEntity"
        );
        BinaryExpression unboxEntity = Expression.Assign(
            unboxedEntityVar,
            Expression.Convert(boxedEntityParam, bindingInfo.EntityType)
        );

        ParameterExpression valueBufferVar = Expression.Variable(valueBufferType, "buffer");

        List<Expression> blockExpressions =
        [
            unboxEntity,
            Expression.Assign(valueBufferVar, Expression.New(valueBufferType)),
        ];

        foreach (ColumnInfo columnInfo in bindingInfo.Columns)
        {
            ConstantExpression key = Expression.Constant(columnInfo);
            MemberExpression value = Expression.Property(
                unboxedEntityVar,
                columnInfo.Property.Name
            );

            IndexExpression index = Expression.MakeIndex(
                valueBufferVar,
                valueBufferType.GetProperty("Item"),
                [key]
            );
            blockExpressions.Add(
                Expression.Assign(index, Expression.Convert(value, typeof(object)))
            );
        }

        blockExpressions.Add(valueBufferVar);

        return Expression
            .Lambda<Func<object, ValueBuffer>>(
                Expression.Block([unboxedEntityVar, valueBufferVar], blockExpressions),
                boxedEntityParam
            )
            .Compile();
    }

    private static Func<ValueBuffer, object> CreatePropertyMaterializer(
        EntityBindingInfo bindingInfo
    )
    {
        ParameterExpression bufferParam = Expression.Parameter(typeof(ValueBuffer), "buffer");
        ParameterExpression instanceVar = Expression.Variable(bindingInfo.EntityType, "instance");

        List<Expression> blockExpressions =
        [
            Expression.Assign(instanceVar, Expression.New(bindingInfo.Constructor)),
        ];

        foreach (ColumnInfo column in bindingInfo.GetOrderedColumns())
        {
            UnaryExpression valueExpr = CreateBufferPropertyBinding(bufferParam, column);
            MemberExpression propertyExpr = Expression.Property(instanceVar, column.Property.Name);
            blockExpressions.Add(Expression.Assign(propertyExpr, valueExpr));
        }

        blockExpressions.Add(instanceVar);

        return Expression
            .Lambda<Func<ValueBuffer, object>>(
                Expression.Block([instanceVar], blockExpressions),
                bufferParam
            )
            .Compile();
    }
}
