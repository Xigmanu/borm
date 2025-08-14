using Borm.Data;

namespace Borm.Model.Metadata;

internal sealed class TableGraphBuilder
{
    private readonly Dictionary<Type, EntityMetadata> _entityInfoMap;
    private readonly Dictionary<EntityMetadata, Table> _tableCache;

    public TableGraphBuilder(IEnumerable<EntityMetadata> entityInfos)
    {
        _entityInfoMap = entityInfos.ToDictionary(e => e.DataType);
        _tableCache = [];
    }

    public IEnumerable<Table> BuildAll()
    {
        foreach (EntityMetadata entityMetadata in _entityInfoMap.Values)
        {
            yield return BuildTableRecursive(entityMetadata);
        }
    }

    private Table BuildTableRecursive(EntityMetadata entityMetadata)
    {
        if (_tableCache.TryGetValue(entityMetadata, out Table? cachedTable))
        {
            return cachedTable;
        }

        Dictionary<ColumnMetadata, Table> columnRelations = [];

        foreach (ColumnMetadata column in entityMetadata.Columns)
        {
            if (column.Reference is null)
            {
                continue;
            }

            if (_entityInfoMap.TryGetValue(column.Reference, out EntityMetadata? dependency))
            {
                Table dependencyTable = BuildTableRecursive(dependency);
                columnRelations[column] = dependencyTable;
            }
            else
            {
                throw new InvalidOperationException(
                    $"No node found for referenced type {column.Reference}"
                );
            }
        }

        Table table = new(entityMetadata, columnRelations);
        return _tableCache[entityMetadata] = table;
    }
}
