using Borm.Data;
using static Borm.Tests.Mocks.TableMocks;

namespace Borm.Tests.Data;

public sealed class TableGraphTest
{
    [Fact]
    public void AddTable_InsertsTableAndMapsItToEntityType_IfTableNotExists()
    {
        // Arrange
        Table table = CreateAddressesTable();
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
        Table addressesTable = CreateAddressesTable();
        Table personsTable = CreatePersonsTable(addressesTable);
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
        // Arrange
        Table table = CreateAddressesTable();
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
        Table table = CreateAddressesTable();
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
        Table addressesTable = CreateAddressesTable();
        Table personsTable = CreatePersonsTable(addressesTable);
        List<Table> tables = [personsTable, addressesTable];
        TableGraph graph = new();
        graph.AddTableRange(tables);

        // Act
        IEnumerable<Table> sorted = graph.TopSort();

        // Assert
        Assert.Equal(addressesTable, sorted.ElementAt(0));
        Assert.Equal(personsTable, sorted.ElementAt(1));
    }
}
