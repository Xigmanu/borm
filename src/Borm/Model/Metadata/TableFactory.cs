using Borm.Data;

namespace Borm.Model.Metadata;

internal static class TableFactory
{
    public static IEnumerable<Table> Create(IEnumerable<EntityNode> nodes)
    {
        return nodes.Select(node => CreateTable(node, nodes));
    }

    private static Table CreateTable(EntityNode node, IEnumerable<EntityNode> nodes)
    {
        Dictionary<IColumn, ITable> relationMap = [];
        foreach (ColumnInfo column in node.Columns)
        {
            if (column.Reference != null)
            {
                EntityNode dependencyNode = nodes.First(n => n.DataType == column.Reference);
                Table dependency = CreateTable(dependencyNode, nodes);
                relationMap[column] = dependency;
            }
        }

        return new Table(node, relationMap);
    }
}
