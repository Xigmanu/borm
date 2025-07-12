using Borm.Extensions;
using System.Diagnostics;
using System.Reflection;

namespace Borm.Schema.Metadata;

internal sealed class EntityBindingInfo
{
    private readonly ColumnInfoCollection _columns;
    private readonly ConstructorInfo _constructor;
    private readonly Type _entityType;

    public EntityBindingInfo(Type entityType, ColumnInfoCollection columns)
    {
        _entityType = entityType;
        _columns = columns;

        ConstructorInfo[] constructors = _entityType.GetConstructors();
        Debug.Assert(constructors.Length > 0);

        EntityConstructorSelector selector = new(_columns, constructors);
        _constructor = selector.Select() ?? constructors[0];
    }

    public ConstructorInfo Constructor => _constructor;
    public Type EntityType => _entityType;
    internal ColumnInfoCollection Columns => _columns;

    public ColumnInfo[] GetOrderedColumns()
    {
        if (_constructor.IsNoArgs())
        {
            return [.. Columns];
        }

        ColumnInfo[] ordered = new ColumnInfo[Columns.Count];
        ParameterInfo[] ctorParams = _constructor.GetParameters();
        for (int i = 0; i < ctorParams.Length; i++)
        {
            ordered[i] = Columns[ctorParams[i].Name!];
        }
        return ordered;
    }
}
