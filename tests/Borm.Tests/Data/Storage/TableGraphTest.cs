using Borm.Data.Storage;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Data.Storage;

public sealed class TableGraphTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void AddTable_InsertsTableAndMapsItToEntityType_IfTableNotExists()
    {
        // Arrange
        Table table = _graph[typeof(AddressEntity)]!;
        TableGraph graph = new();

        // Act
        graph.AddTable(table);
        graph.AddTable(table);

        // Assert
        Assert.Equal(1, graph.TableCount);
    }

    [Fact]
    public void AddTableRange_InsertsTableEnumeration()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        Table personsTable = _graph[typeof(PersonEntity)]!;
        List<Table> tables = [addressesTable, personsTable];
        TableGraph graph = new();

        // Act
        graph.AddTableRange(tables);

        // Assert
        Assert.Equal(tables.Count, graph.TableCount);
    }

    [Fact]
    public void Indexer_ReturnsNull_IfTableNotExists()
    {
        // Act
        Table? actual = _graph[typeof(int)];

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void Indexer_ReturnsTable_IfTableExists()
    {
        // Arrange
        Table table = _graph[typeof(AddressEntity)]!;

        // Act
        Table? actual = _graph[table.EntityMetadata.DataType];

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(table, actual);
    }

    [Fact]
    public void TopSort_ReturnsTopologicallySortedRangeOfTables()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        Table personsTable = _graph[typeof(PersonEntity)]!;

        // Act
        IEnumerable<Table> sorted = _graph.TopSort();

        // Assert
        Assert.Equal(addressesTable, sorted.ElementAt(0));
        Assert.Equal(personsTable, sorted.ElementAt(1));
    }
}
