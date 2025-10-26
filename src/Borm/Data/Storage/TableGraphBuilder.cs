using System.Diagnostics;
using Borm.Model.Metadata;

namespace Borm.Data.Storage;

internal sealed class TableGraphBuilder
{
    private readonly Dictionary<Type, IEntityMetadata> _entityInfoMap;

    public TableGraphBuilder(IEnumerable<IEntityMetadata> entityInfos)
    {
        _entityInfoMap = entityInfos.ToDictionary(e => e.Type);
    }

    public void Build(TableGraph graph)
    {
        Debug.Assert(graph.TableCount == 0);
        foreach (IEntityMetadata entityMetadata in _entityInfoMap.Values)
        {
            BuildTableRecursive(entityMetadata, graph);
        }
    }

    private Table BuildTableRecursive(IEntityMetadata entityMetadata, TableGraph graph)
    {
        Table? cached = graph[entityMetadata.Type];
        if (cached != null)
        {
            return cached;
        }

        Table table = new(entityMetadata);
        graph.AddTable(table);

        foreach (Type? reference in entityMetadata.Columns.Select(column => column.Reference))
        {
            if (reference is null)
            {
                continue;
            }

            if (!_entityInfoMap.TryGetValue(reference, out IEntityMetadata? dependency))
            {
                throw new InvalidOperationException(
                    $"No node found for referenced type {reference}"
                );
            }

            Table parent = BuildTableRecursive(dependency, graph);
            graph.AddTable(parent);
            graph.AddEdge(parent, table);
        }

        return table;
    }
}
