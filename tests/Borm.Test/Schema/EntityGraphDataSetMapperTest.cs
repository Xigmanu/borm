using System.Data;
using Borm.Data;
using Borm.Schema;
using Borm.Schema.Metadata;

namespace Borm.Tests.Schema;

public class EntityGraphDataSetMapperTest
{
    [Fact]
    public void LoadMapping_CreatesAndLoadsTableSchemaIntoDataSet_WithEntityNodeGraphAndEmptyDataSet()
    {
        // Arrange
        EntityNodeGraph nodeGraph = TestData.CreateNodeGraph();
        BormDataSet expectedDataSet = TestData.CreateDataSet();

        BormDataSet actualDataSet = new();
        EntityGraphDataSetMapper mapper = new(nodeGraph);

        // Act
        mapper.LoadMapping(actualDataSet);

        // Assert
        Assert.Equal(expectedDataSet.Tables.Count, actualDataSet.Tables.Count);
        Assert.Equal(expectedDataSet.Relations.Count, actualDataSet.Relations.Count);

        for (int i = 0; i < expectedDataSet.Tables.Count; i++)
        {
            NodeDataTable expectedTable = (NodeDataTable)expectedDataSet.Tables[i];
            NodeDataTable actualTable = (NodeDataTable)actualDataSet.Tables[i];

            Assert.Equal(expectedTable.TableName, actualTable.TableName);
            Assert.Equal(expectedTable.Columns.Count, actualTable.Columns.Count);
            for (int j = 0; j < expectedTable.Columns.Count; j++)
            {
                DataColumn expectedColumn = expectedTable.Columns[j];
                DataColumn actualColumn = actualTable.Columns[j];

                Assert.Equal(expectedColumn.ColumnName, actualColumn.ColumnName);
                Assert.Equal(expectedColumn.DataType, actualColumn.DataType);
                Assert.Equal(expectedColumn.AllowDBNull, actualColumn.AllowDBNull);
            }
        }

        DataRelation expectedRelation = expectedDataSet.Relations[0];
        DataRelation actualRelation = actualDataSet.Relations[0];

        Assert.Equal(expectedRelation.RelationName, actualRelation.RelationName);
        Assert.Equal(
            expectedRelation.ParentColumns[0].ColumnName,
            actualRelation.ParentColumns[0].ColumnName
        );
    }

    private static class TestData
    {
        private static readonly EntityNode FKNode = CreateFkNode();
        private static readonly EntityNode SimpleNode = CreateSimpleNode();

        public static BormDataSet CreateDataSet()
        {
            BormDataSet dataSet = new();

            NodeDataTable simpleTable = CreateTable(SimpleNode);
            NodeDataTable fkTable = CreateTable(FKNode);

            dataSet.Tables.AddRange([simpleTable, fkTable]);

            DataRelation relation = new(
                $"{fkTable.TableName}_{simpleTable.TableName}",
                simpleTable.PrimaryKey[0],
                fkTable.Columns[^1]
            );
            dataSet.Relations.Add(relation);

            return dataSet;
        }

        public static EntityNodeGraph CreateNodeGraph()
        {
            EntityNodeGraph nodeGraph = new();

            nodeGraph.AddSuccessorSet(SimpleNode, []);
            nodeGraph.AddSuccessorSet(FKNode, [SimpleNode]);

            return nodeGraph;
        }

        private static EntityNode CreateFkNode()
        {
            ColumnInfo pk1 = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
            ColumnInfo fkColumn1 = new(
                1,
                "fkCol",
                "FkCol",
                typeof(int),
                Constraints.None,
                typeof(int)
            );

            ColumnInfoCollection columns1 = new([pk1, fkColumn1]);

            return new EntityNode("table2", typeof(int), columns1);
        }

        private static EntityNode CreateSimpleNode()
        {
            ColumnInfo pk0 = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
            ColumnInfo name0 = new(1, "name", "Name", typeof(string), Constraints.None, null);
            ColumnInfo comment0 = new(
                2,
                "comment",
                "Comment",
                typeof(string),
                Constraints.AllowDbNull,
                null
            );

            ColumnInfoCollection columns0 = new([pk0, name0, comment0]);

            return new EntityNode("table1", typeof(int), columns0);
        }

        private static NodeDataTable CreateTable(EntityNode node)
        {
            NodeDataTable table = new(node.Name, node);
            DataColumn[] columns = new DataColumn[node.Columns.Count];
            for (int i = 0; i < columns.Length; i++)
            {
                ColumnInfo columnInfo = node.Columns.ElementAt(i);
                DataColumn column = new(columnInfo.Name, columnInfo.DataType);
                if (!columnInfo.Constraints.HasFlag(Constraints.AllowDbNull))
                {
                    column.AllowDBNull = false;
                }
                columns[i] = column;
            }
            table.Columns.AddRange(columns);
            table.PrimaryKey = [table.Columns[0]];
            return table;
        }
    }
}
