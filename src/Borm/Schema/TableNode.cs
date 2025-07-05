using System.Diagnostics;
using System.Reflection;

namespace Borm.Schema;

internal sealed class TableNode
{
    private readonly IEnumerable<ColumnInfo> _columns;
    private readonly ConstructorInfo? _ctor;
    private readonly Type _dataType;
    private readonly string _name;

    public TableNode(
        string name,
        Type dataType,
        IEnumerable<ColumnInfo> columns,
        ConstructorInfo? ctor
    )
    {
        ArgumentNullException.ThrowIfNull(columns);
        if (!columns.Any())
        {
            throw new ArgumentException("Table must have at least 1 column", nameof(columns));
        }

        _columns = columns;
        _ctor = ctor;
        _dataType = dataType;
        _name = name;
    }

    public IEnumerable<ColumnInfo> Columns => _columns;
    public Type DataType => _dataType;
    public string Name => _name;

    public ColumnInfo GetPrimaryKey()
    {
        foreach (ColumnInfo columnData in _columns)
        {
            if (columnData.Constraints == Constraints.PrimaryKey)
            {
                return columnData;
            }
        }
        throw new InvalidOperationException($"Node {ToString()} has no primary key");
    }

    public object GetPrimaryKeyValue(object entityObj)
    {
        return GetPrimaryKey().GetValue(entityObj)!;
    }

    internal object CreateInstance(object?[] ctorArgs)
    {
        Debug.Assert(ctorArgs != null);
        return _ctor.Invoke(ctorArgs); // TODO handle ctor vs field injection
    }
}
