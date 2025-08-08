using Borm.Data;

namespace Borm.Model.Metadata;

internal sealed class ColumnInfo : IColumn
{
    public ColumnInfo(
        int index,
        string name,
        string propertyName,
        Type dataType,
        Constraints constraints,
        Type? reference
    )
    {
        Index = index;
        Name = name;
        Reference = reference;
        DataType = dataType;
        Constraints = constraints;
        PropertyName = propertyName;
    }

    public Constraints Constraints { get; }
    public Type DataType { get; }

    public int Index { get; }

    public string Name { get; }

    public string PropertyName { get; }

    public Type? Reference { get; }
}
