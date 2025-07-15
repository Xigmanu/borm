namespace Borm.Schema.Metadata;

internal sealed class EntityNode
{
    private readonly ColumnInfoCollection _columns;
    private readonly Type _dataType;
    private readonly string _name; // name is unique for each schema

    public EntityNode(
        string name,
        Type dataType,
        ColumnInfoCollection columns
    )
    {
        if (columns.Count == 0)
        {
            throw new ArgumentException("Table must have at least 1 column", nameof(columns));
        }

        _columns = columns;
        _dataType = dataType;
        _name = name;
        Binding = ConversionBinding.Empty;
    }

    public Type DataType => _dataType;

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
