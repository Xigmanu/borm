namespace Borm.Data;

public sealed record ColumnInfo(string Name, Type DataType, bool IsUnique, bool IsNullable);
