using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Borm.Data;

/// <summary>
/// Represents the schema of a database table.
/// </summary>
public sealed class TableInfo
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name">Name of the table.</param>
    /// <param name="columns">Table columns.</param>
    /// <param name="primaryKey">Primary key column.</param>
    /// <param name="foreignKeyRelations">Mapping of foreign key columns in this table to the parent.</param>
    public TableInfo(
        string name,
        ReadOnlyCollection<ColumnInfo> columns,
        ColumnInfo primaryKey,
        ReadOnlyDictionary<ColumnInfo, TableInfo> foreignKeyRelations
    )
    {
        Name = name;
        Columns = columns;
        PrimaryKey = primaryKey;
        ForeignKeyRelations = foreignKeyRelations;
    }

    public ReadOnlyCollection<ColumnInfo> Columns { get; }
    public ReadOnlyDictionary<ColumnInfo, TableInfo> ForeignKeyRelations { get; }
    public string Name { get; }
    public ColumnInfo PrimaryKey { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not TableInfo other)
        {
            return false;
        }

        return Name.Equals(other.Name)
            && Columns.SequenceEqual(other.Columns)
            && ForeignKeyRelations.SequenceEqual(
                other.ForeignKeyRelations,
                new RelationDictionaryEqualityComparer()
            );
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Columns, PrimaryKey, ForeignKeyRelations);
    }

    private sealed class RelationDictionaryEqualityComparer
        : IEqualityComparer<KeyValuePair<ColumnInfo, TableInfo>>
    {
        public bool Equals(
            KeyValuePair<ColumnInfo, TableInfo> x,
            KeyValuePair<ColumnInfo, TableInfo> y
        )
        {
            return x.Key.Equals(y.Key) && x.Value.Equals(y.Value);
        }

        public int GetHashCode([DisallowNull] KeyValuePair<ColumnInfo, TableInfo> obj)
        {
            return HashCode.Combine(obj.Key, obj.Value);
        }
    }
}
