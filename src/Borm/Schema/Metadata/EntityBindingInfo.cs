using System.Diagnostics;
using System.Reflection;
using Borm.Extensions;

namespace Borm.Schema.Metadata;

internal sealed class EntityBindingInfo
{
    private readonly ColumnInfoCollection _columns;
    private readonly ConstructorInfo _constructor;

    public EntityBindingInfo(Type entityType, ColumnInfoCollection columns)
    {
        EntityType = entityType;
        _columns = columns;

        ConstructorInfo[] constructors = entityType.GetConstructors();
        Debug.Assert(constructors.Length > 0);

        EntityConstructorSelector selector = new(_columns, constructors);
        _constructor = selector.Select() ?? constructors[0];
    }

    public ConstructorInfo Constructor => _constructor;
    public Type EntityType { get; }
    internal ColumnInfoCollection Columns => _columns;

    public ColumnInfo[] GetOrderedColumns()
    {
        if (_constructor.IsNoArgs())
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
