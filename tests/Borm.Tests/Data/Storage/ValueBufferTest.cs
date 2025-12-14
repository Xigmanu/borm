using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage;

public sealed class ValueBufferTest
{
    private readonly ITableGraph _graph = TableGraphMock.Create();

    public static TheoryData<object[]?, bool> TestData =>
        new()
        {
            { null, false },
            { [1], false },
            { [0, "address", DBNull.Value, "city"], false },
            { [0, "foo", DBNull.Value, "city"], false },
            { [0, "address", "address_1", "city"], false },
            { [0, "address", DBNull.Value, "some"], false },
            { [1, "address", DBNull.Value, "city"], true }
        };

    [Fact]
    public void Copy_ReturnsCopyOfBuffer()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        IReadOnlyList<IColumnMetadata> columns = table.Metadata.Columns;
        ValueBuffer buffer = new();
        for (int i = 0; i < columns.Count; i++)
        {
            buffer[columns[i]] = AddressesDummyData[i];
        }

        // Act
        IValueBuffer clone = buffer.Copy();

        // Assert
        Assert.Equal(buffer.Length, clone.Length);
        Assert.Equal(columns.Count, clone.Length);
        foreach (IColumnMetadata column in columns)
        {
            object expected = buffer[column];
            object actual = clone[column];

            Assert.Equal(expected, actual);
        }
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void Equals_ChecksIfBuffersAreEqual_WithEqualSchemas(object[]? values, bool check)
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        IReadOnlyList<IColumnMetadata> columns = table.Metadata.Columns;
        ValueBuffer buffer = new();
        for (int i = 0; i < columns.Count; i++)
        {
            buffer[columns[i]] = AddressesDummyData[i];
        }

        object? other = null;
        if (values != null)
        {
            if (values.Length != 1)
            {
                ValueBuffer otherBuf = new();
                for (int i = 0; i < columns.Count; i++)
                {
                    otherBuf[columns[i]] = values[i];
                }

                other = otherBuf;
            }
            else
            {
                other = values[0];
            }
        }

        // Act
        bool equal = buffer.Equals(other);

        // Assert
        Assert.Equal(check, equal);
    }

    [Fact]
    public void Equals_ReturnsFalse_WithDifferentSchemas()
    {
        // Arrange
        ITable table0 = _graph[typeof(AddressEntity)]!;
        ITable table1 = _graph[typeof(PersonEntity)]!;

        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, table0.Metadata.Columns)
        );
        IValueBuffer other = CreateBuffer(
            MapValuesToColumns(PersonsDummyData, table1.Metadata.Columns)
        );

        // Act
        bool equal = buffer.Equals(other);

        // Assert
        Assert.False(equal);
    }

    [Fact]
    public void Indexer_ReturnsColumnValue_WithColumnMetadata()
    {
        TestBufferColumnValue((buffer, columns) => buffer[columns[^1]], values => values[^1]);
    }

    [Fact]
    public void Indexer_ReturnsColumnValue_WithColumnName()
    {
        TestBufferColumnValue((buffer, columns) => buffer[columns[^1].Name], values => values[^1]);
    }

    [Fact]
    public void PrimaryKey_ReturnsValueOfPrimaryKeyColumn()
    {
        TestBufferColumnValue((buffer, _) => buffer.PrimaryKey, values => values[0]);
    }

    private void TestBufferColumnValue(
        Func<ValueBuffer, IReadOnlyList<IColumnMetadata>, object> columnValueSupplier,
        Func<object[], object> expectedValueSupplier
    )
    {
        // Arrange
        object[] values = [1, "address", DBNull.Value, "city"];
        ITable table = _graph[typeof(AddressEntity)]!;
        IReadOnlyList<IColumnMetadata> columns = table.Metadata.Columns;
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