using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Borm.Data;
using Borm.Extensions;

namespace Borm.Model.Metadata;

[DebuggerTypeProxy(typeof(BindingInfoDebugView))]
internal sealed class BindingInfo
{
    private readonly ColumnMetadataCollection _columns;
    private readonly ConstructorInfo _constructor;
    private readonly Type _entityType;

    public BindingInfo(Type entityType, ColumnMetadataCollection columns)
    {
        _entityType = entityType;
        _columns = columns;

        ConstructorInfo[] constructors = entityType.GetConstructors();
        Debug.Assert(constructors.Length > 0);

        EntityConstructorSelector selector = new(_columns, constructors);
        _constructor = selector.Select() ?? constructors[0];
    }

    public ConversionBinding CreateBinding()
    {
        Func<object, ValueBuffer> converter = CreateEntityValueBufferConverter();
        Func<ValueBuffer, object> materializer = _constructor.IsNoArgs()
            ? CreatePropertyMaterializer()
            : CreateConstructorMaterializer();
        return new ConversionBinding(materializer, converter);
    }

    private static Expression CreateBufferPropertyBinding(
        ParameterExpression bufferParam,
        ColumnMetadata column
    )
    {
        IndexExpression bufferValue = Expression.Property(
            bufferParam,
            "Item",
            Expression.Constant(column)
        );

        UnaryExpression convertValue = Expression.Convert(bufferValue, column.PropertyType);
        if (!column.Constraints.HasFlag(Constraints.AllowDbNull))
        {
            return convertValue;
        }

        BinaryExpression isDbNull = Expression.Equal(
            bufferValue,
            Expression.Constant(DBNull.Value)
        );
        ConstantExpression nullValue = Expression.Constant(null, column.PropertyType);
        return Expression.Condition(isDbNull, nullValue, convertValue);
    }

    private Func<ValueBuffer, object> CreateConstructorMaterializer()
    {
        ParameterExpression bufferParam = Expression.Parameter(typeof(ValueBuffer), "buffer");

        IEnumerable<Expression> args = GetOrderedColumns()
            .Select(column => CreateBufferPropertyBinding(bufferParam, column));
        NewExpression ctorCall = Expression.New(_constructor, args);

        return Expression
            .Lambda<Func<ValueBuffer, object>>(
                Expression.Convert(ctorCall, typeof(object)),
                bufferParam
            )
            .Compile();
    }

    private Func<object, ValueBuffer> CreateEntityValueBufferConverter()
    {
        Type valueBufferType = typeof(ValueBuffer);

        ParameterExpression boxedEntityParam = Expression.Parameter(typeof(object), "entity");
        ParameterExpression unboxedEntityVar = Expression.Variable(_entityType, "typedEntity");
        BinaryExpression unboxEntity = Expression.Assign(
            unboxedEntityVar,
            Expression.Convert(boxedEntityParam, _entityType)
        );

        ParameterExpression valueBufferVar = Expression.Variable(valueBufferType, "buffer");

        List<Expression> blockExpressions =
        [
            unboxEntity,
            Expression.Assign(valueBufferVar, Expression.New(valueBufferType)),
        ];

        foreach (ColumnMetadata column in _columns)
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
                valueBufferType.GetProperty("Item", [typeof(ColumnMetadata)]),
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
            blockExpressions.Add(conditionalAssign);
        }

        blockExpressions.Add(valueBufferVar);

        return Expression
            .Lambda<Func<object, ValueBuffer>>(
                Expression.Block([unboxedEntityVar, valueBufferVar], blockExpressions),
                boxedEntityParam
            )
            .Compile();
    }

    private Func<ValueBuffer, object> CreatePropertyMaterializer()
    {
        ParameterExpression bufferParam = Expression.Parameter(typeof(ValueBuffer), "buffer");
        ParameterExpression instanceVar = Expression.Variable(_entityType, "instance");

        List<Expression> blockExpressions =
        [
            Expression.Assign(instanceVar, Expression.New(_constructor)),
        ];

        foreach (ColumnMetadata column in GetOrderedColumns())
        {
            Expression valueExpr = CreateBufferPropertyBinding(bufferParam, column);
            MemberExpression propertyExpr = Expression.Property(instanceVar, column.PropertyName);
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

    private ColumnMetadata[] GetOrderedColumns()
    {
        if (_constructor.IsNoArgs())
        {
            return [.. _columns];
        }

        ColumnMetadata[] ordered = new ColumnMetadata[_columns.Count];
        ParameterInfo[] ctorParams = _constructor.GetParameters();
        for (int i = 0; i < ctorParams.Length; i++)
        {
            ordered[i] = _columns[ctorParams[i].Name!];
        }
        return ordered;
    }

    [ExcludeFromCodeCoverage(Justification = "Debug view class")]
    internal sealed class BindingInfoDebugView
    {
        private readonly BindingInfo _instance;

        public BindingInfoDebugView(BindingInfo instance)
        {
            _instance = instance;
        }

        public ColumnMetadataCollection Columns => _instance._columns;
        public Type EntityType => _instance._entityType;
    }
}
