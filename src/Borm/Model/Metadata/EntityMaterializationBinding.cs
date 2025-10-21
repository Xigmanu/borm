using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Borm.Data.Storage;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Metadata;

[DebuggerTypeProxy(typeof(BindingInfoDebugView))]
internal sealed class EntityMaterializationBinding
{
    private readonly IReadOnlyList<ColumnMetadata> _columns;
    private readonly EntityTypeInfo _entityTypeInfo;

    public EntityMaterializationBinding(
        EntityTypeInfo entityTypeInfo,
        IReadOnlyList<ColumnMetadata> columns
    )
    {
        _entityTypeInfo = entityTypeInfo;
        _columns = columns;
    }

    public EntityConversionBinding CreateBinding()
    {
        Func<object, ValueBuffer> converter = CreateEntityValueBufferConverter();

        Constructor constructor = GetMaterializationCtor();
        Func<ValueBuffer, object> materializer = constructor.IsDefault
            ? CreatePropertyMaterializer()
            : CreateConstructorMaterializer(constructor);
        return new EntityConversionBinding(materializer, converter);
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

    private Func<ValueBuffer, object> CreateConstructorMaterializer(Constructor constructor)
    {
        ParameterExpression bufferParam = Expression.Parameter(typeof(ValueBuffer), "buffer");

        IEnumerable<Expression> args = GetOrderedColumns(constructor.Parameters)
            .Select(column => CreateBufferPropertyBinding(bufferParam, column));
        Expression ctorCall = constructor.CreateNewInstanceExpression(args);

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
        Type entityType = _entityTypeInfo.Type;

        ParameterExpression boxedEntityParam = Expression.Parameter(typeof(object), "entity");
        ParameterExpression unboxedEntityVar = Expression.Variable(entityType, "typedEntity");
        BinaryExpression unboxEntity = Expression.Assign(
            unboxedEntityVar,
            Expression.Convert(boxedEntityParam, entityType)
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
        ParameterExpression instanceVar = Expression.Variable(_entityTypeInfo.Type, "instance");

        List<Expression> blockExpressions =
        [
            Expression.Assign(instanceVar, Expression.New(_entityTypeInfo.Type)),
        ];

        foreach (ColumnMetadata column in _columns)
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

    private Constructor GetMaterializationCtor()
    {
        IReadOnlyList<Constructor> constructors = _entityTypeInfo.Constructors;

        Constructor? constructor =
            ConstructorSelector.FindMappingCtor(constructors, [.. _columns.Select(col => col.Name)])
            ?? throw new MissingMethodException(
                Strings.InvalidEntityTypeConstructor(_entityTypeInfo.Type.FullName!)
            );

        return constructor;
    }

    private ColumnMetadata[] GetOrderedColumns(IReadOnlyList<MappingMember> ctorParameters)
    {
        ColumnMetadata[] ordered = new ColumnMetadata[_columns.Count];
        for (int i = 0; i < ctorParameters.Count; i++)
        {
            string paramName = ctorParameters[i].MemberName!;
            ordered[i] = _columns.First(col => col.Name == paramName);
        }
        return ordered;
    }

    [ExcludeFromCodeCoverage(Justification = "Debug view class")]
    internal sealed class BindingInfoDebugView
    {
        private readonly EntityMaterializationBinding _instance;

        public BindingInfoDebugView(EntityMaterializationBinding instance)
        {
            _instance = instance;
        }

        public IReadOnlyCollection<ColumnMetadata> Columns => _instance._columns;
        public Type EntityType => _instance._entityTypeInfo.Type;
    }
}
