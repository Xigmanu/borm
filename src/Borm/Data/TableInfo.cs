namespace Borm.Data;

public sealed record TableInfo(
    string Name,
    IEnumerable<ColumnInfo> Columns,
    ColumnInfo PrimaryKey,
    IReadOnlyDictionary<ColumnInfo, TableInfo> ForeignKeyRelations
);
