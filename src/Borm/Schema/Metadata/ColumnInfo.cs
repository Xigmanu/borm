namespace Borm.Schema.Metadata;

internal sealed class ColumnInfo
{
    private readonly Constraints _constraints;
    private readonly Type _dataType;
    private readonly int _index;
    private readonly string _name;
    private readonly Type? _referencedEntityType;

    public ColumnInfo(
        int index,
        string name,
        string propertyName,
        Type dataType,
        Constraints constraints,
        Type? referencedEntityType
    )
    {
        _index = index;
        _name = name;
        _referencedEntityType = referencedEntityType;
        _dataType = dataType;
        _constraints = constraints;
        PropertyName = propertyName;
    }

    public Type DataType => _dataType;

    public int Index => _index;

    public string Name => _name;

    public string PropertyName { get; }
    public Type? ReferencedEntityType => _referencedEntityType;
    internal Constraints Constraints => _constraints;
}
