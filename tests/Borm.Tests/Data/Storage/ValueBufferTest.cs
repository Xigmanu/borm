using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Data.Storage;

public sealed class ValueBufferTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Indexer_ReturnsColumnValue_WithColumnMetadata()
    {
        TestBufferColumnValue((buffer, columns) => buffer[columns[^1]], (values) => values[^1]);
    }

    [Fact]
    public void Indexer_ReturnsColumnValue_WithColumnName()
    {
        TestBufferColumnValue(
            (buffer, columns) => buffer[columns[^1].Name],
            (values) => values[^1]
        );
    }

    [Fact]
    public void PrimaryKey_ReturnsValueOfPrimaryKeyColumn()
    {
        TestBufferColumnValue((buffer, _) => buffer.PrimaryKey, (values) => values[0]);
    }

    private void TestBufferColumnValue(
        Func<ValueBuffer, ColumnMetadataCollection, object> columnValueSupplier,
        Func<object[], object> expectedValueSupplier
    )
    {
        // Arrange
        object[] values = [1, "address", DBNull.Value, "city"];
        Table table = _graph[typeof(AddressEntity)]!;
        ColumnMetadataCollection columns = table.Metadata.Columns;
        ValueBuffer buffer = new();
        for (int i = 0; i < columns.Count; i++)
        {
            buffer[columns[i]] = values[i];
        }

        // Act
        object columnValue = columnValueSupplier(buffer, columns);

        // Assert
        Assert.Equal(expectedValueSupplier(values), columnValue);
    }
}
