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
        EntityMetadata addresses = _graph[typeof(AddressEntity)]!.EntityMetadata;
        EntityMetadata persons = _graph[typeof(PersonEntity)]!.EntityMetadata;
        EntityMetadata employees = _graph[typeof(EmployeeEntity)]!.EntityMetadata;

        TableGraphBuilder builder = new([addresses, persons, employees]);

        // Act
        TableGraph actual = builder.Build();

        // Assert
        Assert.Equal(_graph.TableCount, actual.TableCount);
    }

    [Fact]
    public void Build_ThrowsInvalidOperationException_WhenReferencedEntityDoesNotExist()
    {
        // Arrange
        EntityMetadata persons = _graph[typeof(PersonEntity)]!.EntityMetadata;
        TableGraphBuilder builder = new([persons]);

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Build());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}
