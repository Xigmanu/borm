namespace Borm.Data;

public interface ITable
{
    IEnumerable<IColumn> Columns { get; }
    string Name { get; }
    IColumn PrimaryKey { get; }
    IReadOnlyDictionary<IColumn, ITable> ForeignKeyRelations { get; }
}
