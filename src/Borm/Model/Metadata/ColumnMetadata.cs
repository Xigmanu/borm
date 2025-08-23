namespace Borm.Model.Metadata;

internal sealed class ColumnMetadata
{
    public ColumnMetadata(
        int index,
        string columnName,
        string propertyName,
        Type propertyType,
        Constraints constraints,
        Type? reference
    )
    {
        Index = index;
        Name = columnName;
        Reference = reference;
        DataType =
            propertyType.IsValueType && constraints.HasFlag(Constraints.AllowDbNull)
                ? Nullable.GetUnderlyingType(propertyType)!
                : propertyType;
        Constraints = constraints;
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    public Constraints Constraints { get; }
    public Type DataType { get; }
    public int Index { get; }
    public string Name { get; }
    public string PropertyName { get; }
    public Type PropertyType { get; }
    public Type? Reference { get; }
}
