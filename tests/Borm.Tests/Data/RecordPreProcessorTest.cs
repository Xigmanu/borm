using Borm.Data.Internal;
using Borm.Data.Storage;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data;

public sealed class RecordPreProcessorTest
{
    private readonly ITableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Process_ReturnsProcessedBufferAndExtractsForeignKeys_WithComplexRelationalRecord()
    {
        // Arrange
        ITable addressTable = _graph[typeof(AddressEntity)]!;
        ITable personTable = _graph[typeof(PersonEntity)]!;
        IValueBuffer child = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressTable.Metadata.Columns)
        );
        AddressEntity address = new(1, "address", null, "city");
        IValueBuffer record = CreateBuffer(
            MapValuesToColumns([1, "name", 42.619, address], personTable.Metadata.Columns)
        );

        RecordPreProcessor preProcessor = new(_graph);
        addressTable.Insert(child, 0);

        // Act
        IValueBuffer processed = preProcessor.Process(
            record,
            0,
            out IEnumerable<ResolvedForeignKey> keys
        );

        // Assert
        Assert.NotEqual(record, processed);
        Assert.Equal(child.PrimaryKey, processed[personTable.Metadata.Columns[3]]);

        List<ResolvedForeignKey> keysList = keys.ToList();
        Assert.Single(keysList);
        ResolvedForeignKey key = keysList[0];

        Assert.Equal(address.Id, key.Value);
        Assert.True(key.ChangeExists);
        Assert.True(key.IsComplexRecord);
        Assert.Equal(address, key.RawValue);
        Assert.Equal(addressTable, key.Parent);
    }

    [Fact]
    public void Process_ReturnsProcessedBufferAndExtractsForeignKeys_WithSimpleRecord()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, table.Metadata.Columns)
        );

        RecordPreProcessor preProcessor = new(_graph);

        // Act
        IValueBuffer processed = preProcessor.Process(
            buffer,
            0,
            out IEnumerable<ResolvedForeignKey> keys
        );

        // Assert
        Assert.Equal(buffer, processed);
        Assert.Empty(keys);
    }

    [Fact]
    public void Process_ReturnsProcessedBufferAndExtractsForeignKeys_WithSimpleRelationalRecord()
    {
        // Arrange
        ITable personTable = _graph[typeof(PersonEntity)]!;
        ITable employeeTable = _graph[typeof(EmployeeEntity)]!;
        IValueBuffer child = CreateBuffer(
            MapValuesToColumns(PersonsDummyData, personTable.Metadata.Columns)
        );
        IValueBuffer record = CreateBuffer(
            MapValuesToColumns(EmployeesDummyData, employeeTable.Metadata.Columns)
        );

        RecordPreProcessor preProcessor = new(_graph);
        personTable.Insert(child, 0);

        // Act
        IValueBuffer processed = preProcessor.Process(
            record,
            0,
            out IEnumerable<ResolvedForeignKey> keys
        );

        // Assert
        Assert.Equal(record, processed);

        List<ResolvedForeignKey> keysList = keys.ToList();
        Assert.Single(keysList);
        ResolvedForeignKey key = keysList[0];

        Assert.Equal(child.PrimaryKey, key.Value);
        Assert.True(key.ChangeExists);
        Assert.False(key.IsComplexRecord);
        Assert.Equal(key.Value, key.RawValue);
        Assert.Equal(personTable, key.Parent);
    }
}