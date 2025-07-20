using System.Data;
using Borm.Model.Metadata;
using Borm.Schema.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class ValueBufferTest
{
    [Fact]
    public void FromDataRow_ReturnsValueBuffer_WithEntityNodeAndDataRow()
    {
        // Arrange
        int expected = 42;
        ColumnInfo column = new(0, "foo", "Foo", typeof(int), Constraints.None, null);

        EntityNode node = new("foo", typeof(object), new ColumnInfoCollection([column]));

        DataTable table = new("foo");
        table.Columns.Add("foo", typeof(int));

        DataRow row = table.NewRow();
        row[0] = expected;

        // Act
        ValueBuffer buffer = ValueBuffer.FromDataRow(node, row);

        // Assert
        Assert.Equal(expected, buffer[column]);
    }

    [Fact]
    public void Indexer_SetsAndGetsColumnValue_WithColumnInfoAndValidValue()
    {
        // Arrange
        ColumnInfo column = new(0, "foo", "Foo", typeof(int), Constraints.None, null);
        int value = 42;
        ValueBuffer buffer = new();

        // Act
        buffer[column] = value;
        object actual = buffer[column];

        // Assert
        Assert.Equal(value, actual);
    }

    [Fact]
    public void LoadIntoRow_CopiesRowValuesIntoBuffer_WithDataRow()
    {
        // Arrange
        ColumnInfo column = new(0, "foo", "Foo", typeof(int), Constraints.None, null);
        ValueBuffer buffer = new();
        int expected = 42;
        buffer[column] = expected;

        DataTable table = new("foo");
        table.Columns.Add("foo", typeof(int));

        DataRow row = table.NewRow();

        // Act
        buffer.LoadIntoRow(row);

        // Assert
        Assert.Equal(expected, row[0]);
    }
}
