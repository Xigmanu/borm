using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityNodeGraphTest
{
    [Fact]
    public void GetSuccessors_ReturnsSuccessorNodes_WithPredecessorEntityNode()
    {
        // Arrange
        ColumnInfo column = new(0, "foo", "Foo", typeof(int), Constraints.None, null);
        EntityNode node0 = new("foo", typeof(float), new ColumnInfoCollection([column]));
        EntityNode node1 = new("bar", typeof(double), new ColumnInfoCollection([column]));

        EntityNodeGraph nodeGraph = new();
        nodeGraph.AddSuccessorSet(node0, []);
        nodeGraph.AddSuccessorSet(node1, [node0]);

        // Act
        EntityNode[] successors0 = nodeGraph.GetSuccessors(node0);
        EntityNode[] successors1 = nodeGraph.GetSuccessors(node1);

        // Assert
        Assert.Empty(successors0);
        Assert.Single(successors1);
        Assert.Equal(node0, successors1[0]);
    }

    [Fact]
    public void GetSuccessors_ThrowsNodeNotFoundException_WhenEntityNodeDoesNotExistInGraph()
    {
        // Arrange
        ColumnInfo column = new(0, "foo", "Foo", typeof(int), Constraints.None, null);
        EntityNode node0 = new("foo", typeof(float), new ColumnInfoCollection([column]));
        EntityNode node1 = new("bar", typeof(double), new ColumnInfoCollection([column]));

        EntityNodeGraph nodeGraph = new();
        nodeGraph.AddSuccessorSet(node0, []);

        // Act
        Exception exception = Record.Exception(() => _ = nodeGraph.GetSuccessors(node1));

        // Assert
        Assert.IsType<NodeNotFoundException>(exception);
    }

    [Fact]
    public void Indexer_ReturnsEntityNode_WithEntityNodeDataType()
    {
        // Arrange
        ColumnInfo column = new(0, "foo", "Foo", typeof(int), Constraints.None, null);
        EntityNode node = new("foo", typeof(object), new ColumnInfoCollection([column]));
        EntityNodeGraph nodeGraph = new();
        nodeGraph.AddSuccessorSet(node, []);

        // Act
        EntityNode? actual = nodeGraph[node.DataType];

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(node, actual);
    }

    [Fact]
    public void Indexer_ReturnsNull_WithUnknownEntityType()
    {
        // Arrange
        ColumnInfo column = new(0, "foo", "Foo", typeof(int), Constraints.None, null);
        EntityNode node = new("foo", typeof(object), new ColumnInfoCollection([column]));
        EntityNodeGraph nodeGraph = new();
        nodeGraph.AddSuccessorSet(node, []);

        // Act
        EntityNode? actual = nodeGraph[typeof(int)];

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void ReversedTopSort_ReturnsSortedReversedArrayOfNodes()
    {
        // Arrange
        ColumnInfo column = new(0, "foo", "Foo", typeof(int), Constraints.None, null);
        EntityNode node0 = new("foo", typeof(float), new ColumnInfoCollection([column]));
        EntityNode node1 = new("bar", typeof(double), new ColumnInfoCollection([column]));

        EntityNode[] expected = [node0, node1];

        EntityNodeGraph nodeGraph = new();
        nodeGraph.AddSuccessorSet(node1, [node0]);
        nodeGraph.AddSuccessorSet(node0, []);

        // Act
        EntityNode[] actual = nodeGraph.ReversedTopSort();

        // Assert
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], actual[i]);
        }
    }
}
