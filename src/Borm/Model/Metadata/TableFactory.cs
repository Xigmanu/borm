using Borm.Data;

namespace Borm.Model.Metadata;

internal sealed class TableFactory
{
    private readonly IEnumerable<EntityNode> _nodes;
    private readonly Dictionary<EntityNode, Table> _createdTableMap;

    public TableFactory(IEnumerable<EntityNode> nodes)
    {
        _nodes = nodes;
        _createdTableMap = [];
    }

    public IEnumerable<Table> Create()
    {
        return _nodes.Select(CreateTable);
    }

    private Table CreateTable(EntityNode node)
    {
        if (_createdTableMap.TryGetValue(node, out Table? cached))
        {
            return cached;
        }

        Dictionary<IColumn, ITable> relationMap = [];
        foreach (ColumnInfo column in node.Columns)
        {
            if (column.Reference != null)
            {
                EntityNode dependencyNode = _nodes.First(n => n.DataType == column.Reference);
                Table dependency = CreateTable(dependencyNode);
                relationMap[column] = dependency;
            }
        }

        return _createdTableMap[node] = new Table(node, relationMap);
    }
}
