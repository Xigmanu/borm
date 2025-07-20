using System.Data;
using Borm.Data;
using Borm.Model.Metadata;

namespace Borm.Tests.Mocks;

internal static class NodeDataTableRepositoryTestMocks
{
    public static (BormDataSet, EntityNodeGraph) CreateTestData()
    {
        ColumnInfoCollection columnsA = new(
            [
                new ColumnInfo(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null),
                new ColumnInfo(1, "value", "Value", typeof(string), Constraints.None, null),
            ]
        );
        EntityNode nodeA = new("entityA", typeof(EntityA), columnsA)
        {
            Binding = new ConversionBinding(
                (buffer) =>
                {
                    object arg0 = buffer[columnsA["id"]];
                    object arg1 = buffer[columnsA["value"]];
                    return new EntityA((int)arg0, (string)arg1);
                },
                (obj) =>
                {
                    EntityA entityA = (EntityA)obj;
                    ValueBuffer buffer = new();
                    buffer[columnsA["id"]] = entityA.Id;
                    buffer[columnsA["value"]] = entityA.Value;
                    return buffer;
                }
            ),
        };
        ColumnInfoCollection columnsB = new(
            [
                new ColumnInfo(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null),
                new ColumnInfo(
                    1,
                    "entityA",
                    "EntityA",
                    typeof(int),
                    Constraints.None,
                    typeof(EntityA)
                ),
            ]
        );
        EntityNode nodeB = new("entityB", typeof(EntityB), columnsB)
        {
            Binding = new ConversionBinding(
                (buffer) =>
                {
                    object arg0 = buffer[columnsB["id"]];
                    object arg1 = buffer[columnsB["entityA"]];
                    return new EntityB((int)arg0, (int)arg1);
                },
                (obj) =>
                {
                    EntityB entityB = (EntityB)obj;
                    ValueBuffer buffer = new();
                    buffer[columnsB["id"]] = entityB.Id;
                    buffer[columnsB["entityA"]] = entityB.EntityA;
                    return buffer;
                }
            ),
        };
        ColumnInfoCollection columnsC = new(
            [
                new ColumnInfo(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null),
                new ColumnInfo(
                    1,
                    "entityB",
                    "EntityB",
                    typeof(EntityB),
                    Constraints.None,
                    typeof(EntityB)
                ),
            ]
        );
        EntityNode nodeC = new("entityC", typeof(EntityC), columnsC)
        {
            Binding = new ConversionBinding(
                (buffer) =>
                {
                    object arg0 = buffer[columnsC["id"]];
                    object arg1 = buffer[columnsC["entityB"]];
                    return new EntityC((int)arg0, (EntityB)arg1);
                },
                (obj) =>
                {
                    EntityC entityC = (EntityC)obj;
                    ValueBuffer buffer = new();
                    buffer[columnsC["id"]] = entityC.Id;
                    buffer[columnsC["entityB"]] = entityC.EntityB;
                    return buffer;
                }
            ),
        };

        EntityNodeGraph nodeGraph = new();
        nodeGraph.AddSuccessorSet(nodeA, []);
        nodeGraph.AddSuccessorSet(nodeB, [nodeA]);
        nodeGraph.AddSuccessorSet(nodeC, [nodeB]);

        NodeDataTable tableA = new("entityA", nodeA);
        tableA.Columns.Add(new DataColumn("id", typeof(int)) { AllowDBNull = false });
        tableA.Columns.Add(new DataColumn("value", typeof(string)) { AllowDBNull = false });
        tableA.PrimaryKey = [tableA.Columns[0]];

        NodeDataTable tableB = new("entityB", nodeB);
        tableB.Columns.Add(new DataColumn("id", typeof(int)) { AllowDBNull = false });
        tableB.Columns.Add(new DataColumn("entityA", typeof(int)) { AllowDBNull = false });
        tableB.PrimaryKey = [tableB.Columns[0]];

        NodeDataTable tableC = new("entityC", nodeC);
        tableC.Columns.Add(new DataColumn("id", typeof(int)) { AllowDBNull = false });
        tableC.Columns.Add(new DataColumn("entityB", typeof(int)) { AllowDBNull = false });
        tableC.PrimaryKey = [tableC.Columns[0]];

        BormDataSet dataSet = new();
        dataSet.AddTable(tableA);
        dataSet.AddTable(tableB);
        dataSet.AddTable(tableC);

        dataSet.Relations.Add(
            $"{tableB.TableName}_{tableA.TableName}",
            tableA.PrimaryKey[0],
            tableB.Columns[1]
        );
        dataSet.Relations.Add(
            $"{tableC.TableName}_{tableB.TableName}",
            tableB.PrimaryKey[0],
            tableC.Columns[1]
        );

        return (dataSet, nodeGraph);
    }

    public sealed class EntityA(int id, string value)
    {
        public int Id { get; } = id;
        public string Value { get; } = value;
    }

    public sealed class EntityB(int id, int entityA)
    {
        public int EntityA { get; } = entityA;
        public int Id { get; } = id;
    }

    public sealed class EntityC(int id, EntityB entityB)
    {
        public EntityB EntityB { get; } = entityB;
        public int Id { get; } = id;
    }
}
