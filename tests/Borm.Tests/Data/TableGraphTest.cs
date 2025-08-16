using Borm.Data;
using static Borm.Tests.Mocks.TableMocks;

namespace Borm.Tests.Data;

public sealed class TableGraphTest
{
    [Fact]
    public void AddTable_InsertsTableAndMapsItToEntityType_IfTableNotExists()
    {
        // Arrange
        Table table = AddressesTable;
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
        List<Table> tables = [AddressesTable, PersonsTable];
        TableGraph graph = new();

        // Act
        graph.AddTableRange(tables);

        // Assert
        Assert.Equal(tables.Count, graph.TableCount);
    }

    [Fact]
    public void Indexer_ReturnsNull_IfTableNotExists()
    {
        // Arrange
        Table table = AddressesTable;
        TableGraph graph = new();
        graph.AddTable(table);

        // Act
        Table? actual = graph[typeof(int)];

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void Indexer_ReturnsTable_IfTableExists()
    {
        // Arrange
        Table table = AddressesTable;
        TableGraph graph = new();
        graph.AddTable(table);

        // Act
        Table? actual = graph[table.EntityMetadata.DataType];

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(table, actual);
    }

    [Fact]
    public void TopSort_ReturnsTopologicallySortedRangeOfTables()
    {
        // Arrange
        List<Table> tables = [PersonsTable, AddressesTable];
        TableGraph graph = new();
        graph.AddTableRange(tables);

        // Act
        IEnumerable<Table> sorted = graph.TopSort();

        // Assert
        Assert.Equal(AddressesTable, sorted.ElementAt(0));
        Assert.Equal(PersonsTable, sorted.ElementAt(1));
    }
}
