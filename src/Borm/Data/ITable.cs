namespace Borm.Data;

public interface ITable
{
    IEnumerable<IColumn> Columns { get; }
    public string Name { get; }
    IColumn PrimaryKey { get; }
    IReadOnlyDictionary<IColumn, ITable> Relations { get; }
}
