using System.Collections.ObjectModel;
using Borm.Data;
using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Data.Storage;

public sealed class TableGraphTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void AddChild_AddsChildToTable()
    {
        // Arrange
        Table table = _graph[typeof(AddressEntity)]!;
        Table child = _graph[typeof(PersonEntity)]!;
        TableGraph graph = new();
        graph.AddTable(table);
        graph.AddTable(child);

        // Act
        graph.AddChild(table, child);
        graph.AddChild(table, child);

        // Assert
        Assert.Equal(2, graph.TableCount);
        IEnumerable<Table> children = graph.GetChildren(table);
        Assert.Single(children);
        Assert.Equal(child, children.First());
    }

    [Fact]
    public void AddParent_AddsParentToTable()
    {
        // Arrange
        Table table = _graph[typeof(PersonEntity)]!;
        Table parent = _graph[typeof(AddressEntity)]!;
        TableGraph graph = new();
        graph.AddTable(table);
        graph.AddTable(parent);

        // Act
        graph.AddParent(table, parent);
        graph.AddParent(table, parent);

        // Assert
        Assert.Equal(2, graph.TableCount);
        IEnumerable<Table> children = graph.GetParents(table);
        Assert.Single(children);
        Assert.Equal(parent, children.First());
    }

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
    public void GetChildren_ReturnsEmptyEnumeration_WhenNoChildrenExist()
    {
        // Arrange
        Table table = _graph[typeof(EmployeeEntity)]!;

        // Act
        IEnumerable<Table> children = _graph.GetChildren(table);

        // Assert
        Assert.Empty(children);
    }

    [Fact]
    public void GetChildren_ReturnsTableChildren()
    {
        // Arrange
        Table table = _graph[typeof(AddressEntity)]!;
        Table expectedChild = _graph[typeof(PersonEntity)]!;

        // Act
        IEnumerable<Table> children = _graph.GetChildren(table);

        // Assert
        Assert.Single(children);
        Assert.Equal(expectedChild, children.First());
    }

    [Fact]
    public void GetParents_ReturnsEmptyEnumeration_WhenNoParentsExist()
    {
        // Arrange
        Table table = _graph[typeof(AddressEntity)]!;

        // Act
        IEnumerable<Table> children = _graph.GetParents(table);

        // Assert
        Assert.Empty(children);
    }

    [Fact]
    public void GetParents_ReturnsTableParents()
    {
        // Arrange
        Table table = _graph[typeof(PersonEntity)]!;
        Table expectedParent = _graph[typeof(AddressEntity)]!;

        // Act
        IEnumerable<Table> parents = _graph.GetParents(table);

        // Assert
        Assert.Single(parents);
        Assert.Equal(expectedParent, parents.First());
    }

    [Fact]
    public void GetTableSchema_ReturnsValidSchema_ForComplexTable()
    {
        // Arrange
        Table table = _graph[typeof(PersonEntity)]!;
        Table addressTable = _graph[typeof(AddressEntity)]!;
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
            new Dictionary<ColumnInfo, TableInfo>()
            {
                [tableColumns[^1]] = addressTableSchema,
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
        Table table = _graph[typeof(AddressEntity)]!;
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
        Table[] sorted = [.. _graph.TopSort()];

        // Assert
        Assert.Equal(addressesTable, sorted[0]);
        Assert.Equal(personsTable, sorted[1]);
    }

    private static List<ColumnInfo> CreateTestColumns(Table table)
    {
        List<ColumnInfo> columns = [];
        foreach (ColumnMetadata columnMetadata in table.EntityMetadata.Columns)
        {
            ColumnInfo columnSchema = new(
                columnMetadata.Name,
                columnMetadata.DataType == columnMetadata.Reference
                    ? typeof(int)
                    : columnMetadata.DataType,
                columnMetadata.Constraints.HasFlag(Borm.Model.Constraints.Unique),
                columnMetadata.Constraints.HasFlag(Borm.Model.Constraints.AllowDbNull)
            );
            columns.Add(columnSchema);
        }

        return columns;
    }
}
