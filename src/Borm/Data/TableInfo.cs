namespace Borm.Data;

/// <summary>
/// Represents the schema of a database table.
/// </summary>
/// 
/// <param name="Name">Name of the table.</param>
/// <param name="Columns">Table columns.</param>
/// <param name="PrimaryKey">Primary key column.</param>
/// <param name="ForeignKeyRelations">Mapping of foreign key columns in this table to the parent.</param>
public sealed record TableInfo(
    string Name,
    IEnumerable<ColumnInfo> Columns,
    ColumnInfo PrimaryKey,
    IReadOnlyDictionary<ColumnInfo, TableInfo> ForeignKeyRelations
);
