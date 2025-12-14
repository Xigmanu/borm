using Borm.Util;

namespace Borm.Data;

/// <summary>
///     Represents the schema of a database table.
/// </summary>
public sealed class TableInfo
{
    /// <summary>
    /// </summary>
    /// <param name="name">Name of the table.</param>
    /// <param name="columns">Table columns.</param>
    /// <param name="primaryKey">Primary key column.</param>
    /// <param name="foreignKeyRelations">Mapping of foreign key columns in this table to the parent.</param>
    internal TableInfo(
        string name,
        IReadOnlyList<ColumnInfo> columns,
        ColumnInfo primaryKey,
        IReadOnlyDictionary<ColumnInfo, TableInfo> foreignKeyRelations
    )
    {
        Name = name;
        Columns = columns;
        PrimaryKey = primaryKey;
        ForeignKeyRelations = foreignKeyRelations;
    }

    public IReadOnlyList<ColumnInfo> Columns { get; }
    public IReadOnlyDictionary<ColumnInfo, TableInfo> ForeignKeyRelations { get; }
    public string Name { get; }
    public ColumnInfo PrimaryKey { get; }

    public override bool Equals(object? obj)
    {
        return obj is TableInfo other
               && Name.Equals(other.Name)
               && Columns.SequenceEqual(other.Columns);
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode() ^ Columns.GetSequenceHashCode();
    }
}