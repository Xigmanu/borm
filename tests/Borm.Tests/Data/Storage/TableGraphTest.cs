using System.Collections.ObjectModel;
using Borm.Data;
using Borm.Data.Storage;
using Borm.Model;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Data.Storage;

public sealed class TableGraphTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void AddEdge_AddsParentChildAndChildParentRelations()
    {
        // Arrange
        ITable parent = _graph[typeof(AddressEntity)]!;
        ITable child = _graph[typeof(PersonEntity)]!;
        TableGraph graph = new();

        graph.AddTable(parent);
        graph.AddTable(child);

        // Act
        graph.AddEdge(parent, child);

        // Assert
        IEnumerable<ITable> children0 = graph.GetChildren(parent);
        IEnumerable<ITable> children1 = graph.GetChildren(child);
        IEnumerable<ITable> parents0 = graph.GetParents(child);
        IEnumerable<ITable> parents1 = graph.GetParents(parent);

        Assert.Empty(children1);
        Assert.Empty(parents1);

        Assert.Equal(child, children0.First());
        Assert.Equal(parent, parents0.First());
    }

    [Fact]
    public void AddTable_InsertsTableAndMapsItToEntityType_IfTableNotExists()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        TableGraph graph = new();

        // Act
        graph.AddTable(table);
        graph.AddTable(table);

        // Assert
        Assert.Equal(1, graph.TableCount);
    }

    [Fact]
    public void GetChildren_ReturnsEmptyEnumeration_WhenNoChildrenExist()
    {
        // Arrange
        ITable table = _graph[typeof(EmployeeEntity)]!;

        // Act
        IEnumerable<ITable> children = _graph.GetChildren(table);

        // Assert
        Assert.Empty(children);
    }

    [Fact]
    public void GetChildren_ReturnsTableChildren()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        ITable expectedChild = _graph[typeof(PersonEntity)]!;

        // Act
        List<ITable> children = _graph.GetChildren(table).ToList();

        // Assert
        Assert.Single(children);
        Assert.Equal(expectedChild, children[0]);
    }

    [Fact]
    public void GetParents_ReturnsEmptyEnumeration_WhenNoParentsExist()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;

        // Act
        IEnumerable<ITable> children = _graph.GetParents(table);

        // Assert
        Assert.Empty(children);
    }

    [Fact]
    public void GetParents_ReturnsTableParents()
    {
        // Arrange
        ITable table = _graph[typeof(PersonEntity)]!;
        ITable expectedParent = _graph[typeof(AddressEntity)]!;

        // Act
        List<ITable> parents = _graph.GetParents(table).ToList();

        // Assert
        Assert.Single(parents);
        Assert.Equal(expectedParent, parents[0]);
    }

    [Fact]
    public void GetTableSchema_ReturnsValidSchema_ForComplexTable()
    {
        // Arrange
        ITable table = _graph[typeof(PersonEntity)]!;
        ITable addressTable = _graph[typeof(AddressEntity)]!;
        List<ColumnInfo> addressColumns = CreateTestColumns(addressTable);
        List<ColumnInfo> tableColumns = CreateTestColumns(table);

        TableInfo addressTableSchema = new(
            addressTable.Name,
            new ReadOnlyCollection<ColumnInfo>(addressColumns),
            addressColumns[0],
            new Dictionary<ColumnInfo, TableInfo>().AsReadOnly()
        );

        TableInfo expectedSchema = new(
            table.Name,
            new ReadOnlyCollection<ColumnInfo>(tableColumns),
            tableColumns[0],
            new Dictionary<ColumnInfo, TableInfo>
            {
                [tableColumns[^1]] = addressTableSchema
            }.AsReadOnly()
        );

        // Act
        TableInfo schema = _graph.GetTableSchema(table);

        // Assert
        Assert.Equal(expectedSchema.Name, schema.Name);
        Assert.Equal(expectedSchema.PrimaryKey, schema.PrimaryKey);
        Assert.Equal(expectedSchema.Columns, schema.Columns);
        IReadOnlyDictionary<ColumnInfo, TableInfo> actualRelations = schema.ForeignKeyRelations;
        foreach (KeyValuePair<ColumnInfo, TableInfo> kvp in expectedSchema.ForeignKeyRelations)
        {
            Assert.True(actualRelations.ContainsKey(kvp.Key));
            Assert.Equal(actualRelations[kvp.Key], kvp.Value);
        }
    }

    [Fact]
    public void GetTableSchema_ReturnsValidSchema_ForSimpleTable()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        List<ColumnInfo> columns = CreateTestColumns(table);

        TableInfo expectedSchema = new(
            table.Name,
            new ReadOnlyCollection<ColumnInfo>(columns),
            columns[0],
            new Dictionary<ColumnInfo, TableInfo>().AsReadOnly()
        );

        // Act
        TableInfo schema = _graph.GetTableSchema(table);

        // Assert
        Assert.Equal(expectedSchema.Name, schema.Name);
        Assert.Equal(expectedSchema.PrimaryKey, schema.PrimaryKey);
        Assert.Equal(expectedSchema.Columns, schema.Columns);
        Assert.Equal(expectedSchema.ForeignKeyRelations, schema.ForeignKeyRelations);
    }

    [Fact]
    public void Indexer_ReturnsNull_IfTableNotExists()
    {
        // Act
        ITable? actual = _graph[typeof(int)];

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void Indexer_ReturnsTable_IfTableExists()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;

        // Act
        ITable? actual = _graph[table.Metadata.Type];

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(table, actual);
    }

    [Fact]
    public void TopSort_ReturnsTopologicallySortedRangeOfTables()
    {
        // Arrange
        ITable addressesTable = _graph[typeof(AddressEntity)]!;
        ITable personsTable = _graph[typeof(PersonEntity)]!;

        // Act
        ITable[] sorted = [.. _graph.TopSort()];

        // Assert
        Assert.Equal(addressesTable, sorted[0]);
        Assert.Equal(personsTable, sorted[1]);
    }

    private static List<ColumnInfo> CreateTestColumns(ITable table)
    {
        List<ColumnInfo> columns = [];
        columns.AddRange(table.Metadata.Columns.Select(columnMetadata => new ColumnInfo(columnMetadata.Name,
            columnMetadata.DataType.UnderlyingType == columnMetadata.Reference
                ? typeof(int)
                : columnMetadata.DataType.UnderlyingType, columnMetadata.Constraints.HasFlag(Constraints.Unique),
            columnMetadata.Constraints.HasFlag(Constraints.AllowDbNull))));

        return columns;
    }
}