using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage;

public sealed class BufferPreProcessorTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void ResolveForeignKeys_ReturnsEmptyListAndSameBuffer_WhenValueBufferIsSimple()
    {
        // Arrange
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, _graph[typeof(AddressEntity)]!.Metadata.Columns)
        );
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
        AssertBuffersAreEqual(buffer, processed);
    }

    [Fact]
    public void ResolveForeignKeys_ReturnsResolvedKeysAndModifiedBuffer_WithComplexRelationalBuffer()
    {
        // Arrange
        ITable addressTable = _graph[typeof(AddressEntity)]!;
        ITable personTable = _graph[typeof(PersonEntity)]!;
        long initialTxId = -1;

        IValueBuffer addressBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressTable.Metadata.Columns)
        );
        addressTable.Tracker.PendChange(ChangeFactory.Initial(addressBuffer, initialTxId));
        addressTable.Tracker.AcceptPendingChanges(initialTxId);

        AddressEntity address = new(1, "address", null, "city");
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "name", 42.619, address], personTable.Metadata.Columns)
        );

        ResolvedForeignKey expectedResolvedKey = new(
            addressTable,
            addressBuffer.PrimaryKey,
            address,
            true,
            true
        );
        IValueBuffer expectedProcessedBuffer = CreateBuffer(
            MapValuesToColumns(PersonsDummyData, personTable.Metadata.Columns)
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
        AssertBuffersAreEqual(expectedProcessedBuffer, processed);
    }

    [Fact]
    public void ResolveForeignKeys_ReturnsResolvedKeysAndSameBuffer_WithSimpleRelationalBuffer()
    {
        // Arrange
        ITable employeeTable = _graph[typeof(EmployeeEntity)]!;
        ITable personTable = _graph[typeof(PersonEntity)]!;
        long initialTxId = -1;

        IValueBuffer personBuffer = CreateBuffer(
            MapValuesToColumns(PersonsDummyData, personTable.Metadata.Columns)
        );
        personTable.Tracker.PendChange(ChangeFactory.Initial(personBuffer, initialTxId));
        personTable.Tracker.AcceptPendingChanges(initialTxId);

        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(EmployeesDummyData, employeeTable.Metadata.Columns)
        );

        ResolvedForeignKey expectedResolvedKey = new(
            personTable,
            personBuffer.PrimaryKey,
            personBuffer.PrimaryKey,
            false,
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
        AssertBuffersAreEqual(buffer, processed);
    }

    private static void AssertBuffersAreEqual(IValueBuffer expected, IValueBuffer actual)
    {
        foreach ((IColumnMetadata column, object value) in expected)
        {
            object actual1 = actual[column];
            Assert.Equal(value, actual1);
        }
    }
}