using System.Linq.Expressions;
using System.Reflection;

namespace Borm.Schema.Metadata;

internal sealed class EntityBindingInfo
{
    private readonly ColumnInfoCollection _columns;
    private readonly ConstructorInfo _constructor;
    private readonly Type _entityType;

    public EntityBindingInfo(
        Type entityType,
        ColumnInfoCollection columns,
        ConstructorInfo constructor
    )
    {
        _entityType = entityType;
        _columns = columns;
        _constructor = constructor;
    }

    public ConstructorInfo Constructor => _constructor;
    public Type EntityType => _entityType;
    internal ColumnInfoCollection Columns => _columns;

    public static UnaryExpression CreateBufferPropertyBinding(
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

    public ColumnInfo[] GetOrderedColumns()
    {
        if (_constructor == _entityType.GetConstructor(Type.EmptyTypes))
        {
            return [.. _columns];
        }

        ColumnInfo[] ordered = new ColumnInfo[_columns.Count];
        ParameterInfo[] ctorParams = _constructor.GetParameters();
        for (int i = 0; i < ctorParams.Length; i++)
        {
            ordered[i] = _columns[ctorParams[i].Name!];
        }
        return ordered;
    }
}
