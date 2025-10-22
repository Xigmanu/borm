using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data.Storage;

public sealed class BufferPreProcessorTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void ResolveForeignKeys_ReturnsEmptyListAndSameBuffer_WhenValueBufferIsSimple()
    {
        // Arrange
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, _graph[typeof(AddressEntity)]!);
        long txId = 0;
        BufferPreProcessor preProcessor = new(_graph);

        // Act
        List<ResolvedForeignKey> resolved = preProcessor.ResolveForeignKeys(
            buffer,
            txId,
            out IValueBuffer processed
        );

        // Assert
        Assert.Empty(resolved);
        Assert.Equal(buffer, processed);
    }

    [Fact]
    public void ResolveForeignKeys_ReturnsResolvedKeysAndModifiedBuffer_WithComplexRelationalBuffer()
    {
        // Arrange
        Table addressTable = _graph[typeof(AddressEntity)]!;
        Table personTable = _graph[typeof(PersonEntity)]!;
        long initialTxId = -1;

        ValueBuffer addressBuffer = CreateBuffer(AddressesDummyData, addressTable);
        addressTable.Tracker.PendChange(Change.Initial(addressBuffer, initialTxId));
        addressTable.Tracker.AcceptPendingChanges(initialTxId);

        AddressEntity address = new(1, "address", null, "city");
        ValueBuffer buffer = CreateBuffer([1, "name", 42.619, address], personTable);

        ResolvedForeignKey expectedResolvedKey = new(
            addressTable,
            addressBuffer.PrimaryKey,
            address,
            IsComplexRecord: true,
            true
        );
        ValueBuffer expectedProcessedBuffer = CreateBuffer(PersonsDummyData, personTable);

        BufferPreProcessor preProcessor = new(_graph);

        // Act
        List<ResolvedForeignKey> resolved = preProcessor.ResolveForeignKeys(
            buffer,
            initialTxId,
            out IValueBuffer processed
        );

        // Assert
        Assert.Single(resolved);
        Assert.Equal(expectedResolvedKey, resolved[0]);
        Assert.Equal(expectedProcessedBuffer, processed);
    }

    [Fact]
    public void ResolveForeignKeys_ReturnsResolvedKeysAndSameBuffer_WithSimpleRelationalBuffer()
    {
        // Arrange
        Table employeeTable = _graph[typeof(EmployeeEntity)]!;
        Table personTable = _graph[typeof(PersonEntity)]!;
        long initialTxId = -1;

        ValueBuffer personBuffer = CreateBuffer(PersonsDummyData, personTable);
        personTable.Tracker.PendChange(
            Change.Initial(personBuffer, initialTxId)
        );
        personTable.Tracker.AcceptPendingChanges(initialTxId);

        ValueBuffer buffer = CreateBuffer(EmployeesDummyData, employeeTable);

        ResolvedForeignKey expectedResolvedKey = new(
            personTable,
            personBuffer.PrimaryKey,
            personBuffer.PrimaryKey,
            IsComplexRecord: false,
            true
        );

        BufferPreProcessor preProcessor = new(_graph);

        // Act
        List<ResolvedForeignKey> resolved = preProcessor.ResolveForeignKeys(
            buffer,
            initialTxId,
            out IValueBuffer processed
        );

        // Assert
        Assert.Single(resolved);
        Assert.Equal(expectedResolvedKey, resolved[0]);
        Assert.Equal(buffer, processed);
    }
}
