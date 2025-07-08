using System.Data;
using Borm.Data;
using Borm.Schema.Metadata;

namespace Borm.Schema;

internal sealed class EntityGraphDataSetMapper
{
    private readonly EntityNodeGraph _nodeGraph;
    private readonly Dictionary<EntityNode, NodeDataTable> _nodeTableMap;

    public EntityGraphDataSetMapper(EntityNodeGraph nodeGraph)
    {
        _nodeGraph = nodeGraph;
        _nodeTableMap = [];
    }

    public void LoadMapping(BormDataSet dataSet)
    {
        EntityNode[] topSortedNodes = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < topSortedNodes.Length; i++)
        {
            EntityNode node = topSortedNodes[i];
            CreateNodeTable(node);
            dataSet.Tables.Add(_nodeTableMap[node]);
        }

        foreach (NodeDataTable table in dataSet.Tables)
        {
            DataRelation[] relations = CreateDataRelations(table);
            if (relations.Length != 0)
            {
                dataSet.Relations.AddRange(relations);
            }
        }
    }

    private DataRelation[] CreateDataRelations(NodeDataTable table)
    {
        EntityNode node = table.Node;
        EntityNode[] successors = _nodeGraph.GetSuccessors(node);
        DataRelation[] relations = new DataRelation[successors.Length];
        for (int i = 0; i < successors.Length; i++)
        {
            EntityNode successor = successors[i];

            ColumnInfo nodeForeignKey = node.Columns.First(column =>
                column.ReferencedEntityType == successor.DataType
            );
            NodeDataTable parentTable = _nodeTableMap[successor];
            DataColumn parentPrimaryKey = parentTable.PrimaryKey[0];

            DataColumn foreignKey = new($"{nodeForeignKey.Name}", parentPrimaryKey.DataType);
            table.Columns.Add(foreignKey);

            relations[i] = new DataRelation(
                $"{table.TableName}_{parentTable.TableName}",
                parentPrimaryKey,
                foreignKey
            );
        }

        return relations;
    }

    private void CreateNodeTable(EntityNode node)
    {
        NodeDataTable table = new(node.Name, node);
        DataColumn[] columns = new DataColumn[node.Columns.Count];
        DataColumn? primaryKey = null;

        foreach (ColumnInfo columnInfo in node.Columns)
        {
            if (columnInfo.ReferencedEntityType != null)
            {
                continue;
            }

            DataColumn column = new(columnInfo.Name, columnInfo.DataType);
            columns[columnInfo.Index] = column;
            column.AllowDBNull = columnInfo.Constraints.HasFlag(Constraints.AllowDbNull);

            if (columnInfo.Constraints.HasFlag(Constraints.PrimaryKey))
            {
                primaryKey = column;
            }
        }

        table.Columns.AddRange(columns);
        table.PrimaryKey = [primaryKey!];

        _nodeTableMap[node] = table;
    }
}
