using System.Diagnostics;

namespace Borm.Schema.Metadata;

[DebuggerDisplay("Name = {Name}, DataType = {DataType.FullName}")]
internal sealed class EntityNode
{
    private readonly ColumnInfoCollection _columns;
    private readonly string _name;

    public EntityNode(string name, Type dataType, ColumnInfoCollection columns)
    {
        if (columns.Count == 0)
        {
            throw new ArgumentException("Table must have at least 1 column", nameof(columns));
        }

        _columns = columns;
        DataType = dataType;
        _name = name;
        Binding = ConversionBinding.Empty;
    }

    public Type DataType { get; }

    public string Name => _name;

    internal ConversionBinding Binding { get; set; }

    internal ColumnInfoCollection Columns => _columns;

    public override bool Equals(object? obj)
    {
        return obj is EntityNode other && _name == other.Name;
    }

    public override int GetHashCode()
    {
        return _name.GetHashCode();
    }

    public ColumnInfo GetPrimaryKey()
    {
        return _columns.FirstOrDefault(column => column.Constraints == Constraints.PrimaryKey)
            ?? throw new InvalidOperationException($"Node {_name} has no primary key");
    }
}
