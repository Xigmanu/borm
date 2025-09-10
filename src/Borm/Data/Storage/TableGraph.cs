using System.Diagnostics;

namespace Borm.Data.Storage;

[DebuggerDisplay("TableCount = {TableCount}")]
internal sealed class TableGraph
{
    private readonly Dictionary<Type, Table> _tables;

    public TableGraph()
    {
        _tables = [];
    }

    public int TableCount => _tables.Count;

    public Table? this[Type entityType]
    {
        get
        {
            _ = _tables.TryGetValue(entityType, out Table? table);
            return table;
        }
    }

    public void AddTable(Table table)
    {
        Type entityType = table.EntityMetadata.DataType;
        if (!_tables.ContainsKey(entityType))
        {
            _tables[entityType] = table;
        }
    }

    public void AddTableRange(IEnumerable<Table> tables)
    {
        foreach (Table table in tables)
        {
            AddTable(table);
        }
    }

    public IEnumerable<Table> TopSort()
    {
        List<Table> result = [];
        HashSet<Table> visited = [];

        void Visit(Table table)
        {
            if (!visited.Add(table))
            {
                return;
            }

            foreach (Table dependency in table.ParentRelations.Values)
            {
                Visit(dependency);
            }

            result.Add(table);
        }

        foreach (Table table in _tables.Values)
        {
            Visit(table);
        }

        return result;
    }
}
