using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Data.Storage;

public sealed class TableGraphBuilderTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Build_BuildsTableGraph()
    {
        // Arrange
        IEntityMetadata addresses = _graph[typeof(AddressEntity)]!.Metadata;
        IEntityMetadata persons = _graph[typeof(PersonEntity)]!.Metadata;
        IEntityMetadata employees = _graph[typeof(EmployeeEntity)]!.Metadata;

        TableGraphBuilder builder = new([addresses, persons, employees]);

        // Act
        TableGraph actual = new();
        builder.Build(actual);

        // Assert
        Assert.Equal(_graph.TableCount, actual.TableCount);
    }

    [Fact]
    public void Build_ThrowsInvalidOperationException_WhenReferencedEntityDoesNotExist()
    {
        // Arrange
        IEntityMetadata persons = _graph[typeof(PersonEntity)]!.Metadata;
        TableGraphBuilder builder = new([persons]);
        TableGraph graph = new();

        // Act
        Exception? exception = Record.Exception(() => builder.Build(graph));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}
