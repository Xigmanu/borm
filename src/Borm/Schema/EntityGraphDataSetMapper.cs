using System.Data;
using Borm.Data;
using Borm.Schema.Metadata;

namespace Borm.Schema;

internal sealed class EntityGraphDataSetMapper
{
    private readonly EntityNodeGraph _nodeGraph;
    private readonly Dictionary<EntityNode, NodeDataTable> _nodeTableMap;
    private readonly List<PendingDataRelation> _pendingRelations;

    public EntityGraphDataSetMapper(EntityNodeGraph nodeGraph)
    {
        _nodeGraph = nodeGraph;
        _nodeTableMap = [];
        _pendingRelations = [];
    }

    public void LoadMapping(BormDataSet dataSet)
    {
        EntityNode[] topSortedNodes = _nodeGraph.ReversedTopSort();
        for (int i = 0; i < topSortedNodes.Length; i++)
        {
            EntityNode current = topSortedNodes[i];
            CreateNodeTable(current);
            CreatePendingRelations(current);
        }

        foreach (NodeDataTable dataTable in _nodeTableMap.Values)
        {
            dataSet.AddTable(dataTable);
        }

        _pendingRelations.ForEach(pendingRelation =>
            dataSet.Relations.Add(PendingDataRelation.ToDataRelation(pendingRelation))
        );
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

    private void CreatePendingRelations(EntityNode node)
    {
        NodeDataTable childTable = _nodeTableMap[node];
        EntityNode[] successors = _nodeGraph.GetSuccessors(node);
        for (int i = 0; i < successors.Length; i++)
        {
            EntityNode successor = successors[i];

            ColumnInfo nodeForeignKey = node.Columns.First(column =>
                column.ReferencedEntityType == successor.DataType
            );
            NodeDataTable parentTable = _nodeTableMap[successor];
            DataColumn parentPrimaryKey = parentTable.PrimaryKey[0];

            DataColumn tableForeignKey = new($"{nodeForeignKey.Name}", parentPrimaryKey.DataType);
            childTable.Columns.Add(tableForeignKey);

            PendingDataRelation pendingRelation = new(
                $"{childTable.TableName}_{parentTable.TableName}",
                parentPrimaryKey,
                tableForeignKey
            );
            _pendingRelations.Add(pendingRelation);
        }
    }

    private sealed record PendingDataRelation(
        string RelationName,
        DataColumn PKColumn,
        DataColumn FKColumn
    )
    {
        public static DataRelation ToDataRelation(PendingDataRelation pendingDataRelation) =>
            new(
                pendingDataRelation.RelationName,
                pendingDataRelation.PKColumn,
                pendingDataRelation.FKColumn
            );
    }
}
