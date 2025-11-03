using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Model.Metadata;

public class ColumnMetadataListTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Contains_ReturnsIfColumnExistsInList()
    {
        // Arrange
        IReadOnlyList<IColumnMetadata> columns = _graph[typeof(AddressEntity)]!.Metadata.Columns;
        IColumnMetadata expected = columns[0];
        ColumnMetadataList columnsList = new(columns);

        // Act
        bool contains = columnsList.Contains(expected);

        // Assert
        Assert.True(contains);
    }

    [Fact]
    public void Indexer_ReturnsColumnInfo_WithColumnIndex()
    {
        // Arrange
        IReadOnlyList<IColumnMetadata> columns = _graph[typeof(AddressEntity)]!.Metadata.Columns;
        int idx = 0;
        string expected = columns[idx].Name;
        ColumnMetadataList columnsList = new(columns);

        // Act
        IColumnMetadata actual = columnsList[idx];

        // Assert
        Assert.Equal(expected, actual.Name);
    }

    [Fact]
    public void Indexer_ReturnsColumnInfo_WithColumnName()
    {
        // Arrange
        IReadOnlyList<IColumnMetadata> columns = _graph[typeof(AddressEntity)]!.Metadata.Columns;
        string expected = columns[0].Name;
        ColumnMetadataList columnsList = new(columns);

        // Act
        IColumnMetadata actual = columnsList[expected];

        // Assert
        Assert.Equal(expected, actual.Name);
    }

    [Fact]
    public void Indexer_ThrowsKeyNotFoundException_WhenColumnDoesNotExist()
    {
        // Arrange
        IReadOnlyList<IColumnMetadata> columns = _graph[typeof(AddressEntity)]!.Metadata.Columns;
        ColumnMetadataList columnsList = new(columns);

        // Act
        Exception? exception = Record.Exception(() => _ = columnsList["foo"]);

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<KeyNotFoundException>(exception);
    }
}
