using System.Diagnostics;
using Borm.Properties;

namespace Borm.Model.Metadata;

[DebuggerDisplay("Name = {Name}, DataType = {DataType.FullName}")]
internal sealed class EntityNode
{
    private readonly ColumnInfoCollection _columns;
    private readonly string _name;

    public EntityNode(string name, Type dataType, ColumnInfoCollection columns)
    {
        if (columns.Count == 0)
        {
            throw new ArgumentException(Strings.EmptyColumnCollection(name), nameof(columns));
        }

        _columns = columns;
        DataType = dataType;
        _name = name;
        Binding = ConversionBinding.Empty;
    }

    public Type DataType { get; }

    public string Name => _name;

    public ColumnInfo PrimaryKey
    {
        get
        {
            return _columns.FirstOrDefault(column => column.Constraints == Constraints.PrimaryKey)
                ?? throw new InvalidOperationException(Strings.MissingPrimaryKey(_name));
        }
    }

    internal ConversionBinding Binding { get; set; }
    internal ColumnInfoCollection Columns => _columns;
    internal Action<object>? Validator { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is EntityNode other && _name == other.Name;
    }

    public override int GetHashCode()
    {
        return _name.GetHashCode();
    }
}
