using Borm.Data;

namespace Borm.Model.Metadata;

internal sealed class TableGraphBuilder
{
    private readonly Dictionary<Type, EntityNode> _entityNodeMap;
    private readonly Dictionary<EntityNode, Table> _tableCache;

    public TableGraphBuilder(IEnumerable<EntityNode> nodes)
    {
        _entityNodeMap = nodes.ToDictionary(node => node.DataType);
        _tableCache = [];
    }

    public IEnumerable<Table> BuildAll()
    {
        foreach (EntityNode node in _entityNodeMap.Values)
        {
            yield return BuildTableRecursive(node);
        }
    }

    private Table BuildTableRecursive(EntityNode node)
    {
        if (_tableCache.TryGetValue(node, out Table? cachedTable))
        {
            return cachedTable;
        }

        Dictionary<IColumn, ITable> columnRelations = [];

        foreach (ColumnInfo column in node.Columns)
        {
            if (column.Reference is null)
            {
                continue;
            }

            if (_entityNodeMap.TryGetValue(column.Reference, out EntityNode? dependencyNode))
            {
                Table dependencyTable = BuildTableRecursive(dependencyNode);
                columnRelations[column] = dependencyTable;
            }
            else
            {
                throw new InvalidOperationException($"No node found for referenced type {column.Reference}");
            }
        }

        Table table = new(node, columnRelations);
        return _tableCache[node] = table;
    }
}
