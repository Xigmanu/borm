namespace Borm.Data;

/// <summary>
/// Represents metadata about a column in a database table.
/// </summary>
///
public sealed class ColumnInfo
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="name">Column name</param>
    /// <param name="dataType">Column's data type</param>
    /// <param name="isUnique">Flag that indicates whether the value in this column is unique across the entire table.</param>
    /// <param name="isNullable">Flag that indicates whether the value of a column can be null.</param>
    public ColumnInfo(string name, Type dataType, bool isUnique, bool isNullable)
    {
        Name = name;
        DataType = dataType;
        IsUnique = isUnique;
        IsNullable = isNullable;
    }

    public Type DataType { get; }
    public bool IsNullable { get; }
    public bool IsUnique { get; }
    public string Name { get; }

    public override bool Equals(object? obj)
    {
        return obj is ColumnInfo other
            && Name.Equals(other.Name)
            && DataType.Equals(other.DataType)
            && IsUnique == other.IsUnique
            && IsNullable == other.IsNullable;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, DataType, IsUnique, IsNullable);
    }
}
