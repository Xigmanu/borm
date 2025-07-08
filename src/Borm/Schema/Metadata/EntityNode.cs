namespace Borm.Schema.Metadata;

internal sealed class EntityNode
{
    private readonly ColumnInfoCollection _columns;
    private readonly Type _dataType;
    private readonly string _name;

    public EntityNode(
        string name,
        Type dataType,
        ColumnInfoCollection columns,
        EntityConversionBinding binding
    )
    {
        ArgumentNullException.ThrowIfNull(columns);
        if (columns.Count == 0)
        {
            throw new ArgumentException("Table must have at least 1 column", nameof(columns));
        }

        _columns = columns;
        _dataType = dataType;
        _name = name;
        Binding = binding;
    }

    public Type DataType => _dataType;
    public string Name => _name;
    internal EntityConversionBinding Binding { get; }
    internal ColumnInfoCollection Columns => _columns;

    public ColumnInfo GetPrimaryKey()
    {
        return _columns.FirstOrDefault(column => column.Constraints == Constraints.PrimaryKey)
            ?? throw new InvalidOperationException($"Node {_name} has no primary key");
    }
}
