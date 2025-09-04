namespace Borm.Data;

/// <summary>
/// Represents metadata about a column in a database table.
/// </summary>
/// 
/// <param name="Name">Column name</param>
/// <param name="DataType">Column's data type</param>
/// <param name="IsUnique">Flag that indicates whether the value in this column is unique across the entire table.</param>
/// <param name="IsNullable">Flag that indicates whether the value of a column can be null.</param>
public sealed record ColumnInfo(string Name, Type DataType, bool IsUnique, bool IsNullable);
