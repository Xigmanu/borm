using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityNodeGraphFactoryTest
{
    [Fact]
    public void Create_ReturnsNodeGraphWithCorrectRelations_WithEntityNodeEnumeration()
    {
        // Arrange
        Type[] entityTypes = [typeof(bool), typeof(decimal)];

        EntityNode node0 = new(
            "node0",
            entityTypes[0],
            new ColumnInfoCollection(
                [new ColumnInfo(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null)]
            )
        );
        EntityNode node1 = new(
            "node1",
            entityTypes[1],
            new ColumnInfoCollection(
                [
                    new ColumnInfo(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null),
                    new ColumnInfo(1, "fk", "Fk", typeof(int), Constraints.None, typeof(bool)),
                ]
            )
        );
        EntityNode[] nodes = [node0, node1];

        // Act
        EntityNodeGraph graph = EntityNodeGraphFactory.Create(nodes);

        // Assert
        Assert.Equal(nodes.Length, graph.NodeCount);
        for (int i = 0; i < entityTypes.Length; i++)
        {
            EntityNode? actualNode = graph[entityTypes[i]];

            Assert.NotNull(actualNode);
            Assert.Equal(nodes[i], actualNode);
        }
    }
}
