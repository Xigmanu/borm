using Borm.Data;

namespace Borm.Model.Metadata;

internal sealed class TableGraphBuilder
{
    private readonly Dictionary<Type, EntityInfo> _entityInfoMap;
    private readonly Dictionary<EntityInfo, Table> _tableCache;

    public TableGraphBuilder(IEnumerable<EntityInfo> entityInfos)
    {
        _entityInfoMap = entityInfos.ToDictionary(e => e.DataType);
        _tableCache = [];
    }

    public IEnumerable<Table> BuildAll()
    {
        foreach (EntityInfo entityInfo in _entityInfoMap.Values)
        {
            yield return BuildTableRecursive(entityInfo);
        }
    }

    private Table BuildTableRecursive(EntityInfo entityInfo)
    {
        if (_tableCache.TryGetValue(entityInfo, out Table? cachedTable))
        {
            return cachedTable;
        }

        Dictionary<IColumn, ITable> columnRelations = [];

        foreach (Column column in entityInfo.Columns)
        {
            if (column.Reference is null)
            {
                continue;
            }

            if (_entityInfoMap.TryGetValue(column.Reference, out EntityInfo? dependency))
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

        Table table = new(entityInfo, columnRelations);
        return _tableCache[entityInfo] = table;
    }
}
