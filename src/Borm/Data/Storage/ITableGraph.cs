namespace Borm.Data.Storage;

internal interface ITableGraph
{
    IEnumerable<ITable> GetChildren(ITable table);
    IEnumerable<ITable> GetParents(ITable table);
    TableInfo GetSchema(ITable table);
    IEnumerable<ITable> TopSort();
    ITable? this[Type entityType] { get; }
    int TableCount { get; }
}