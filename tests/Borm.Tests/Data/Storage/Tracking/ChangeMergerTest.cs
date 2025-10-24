using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data.Storage.Tracking;

public sealed class ChangeMergerTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void CommitMerge_ReturnsCommittedMergedChange()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer initBuffer = CreateBuffer(AddressesDummyData, addressesTable);
        long initTxId = 0;
        Change initChange = Change.NewChange(initBuffer, initTxId);

        long txId = 1;
        ValueBuffer buffer = CreateBuffer([1, "address", "address_1", "city"], addressesTable);
        Change incoming = initChange.Update(buffer, txId);

        // Act
        Change? merged = Merger.CommitMerge(initChange, incoming);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(incoming.Record, merged.Record);
        Assert.Equal(incoming.WriteId, merged.ReadId);
        Assert.Equal(incoming.WriteId, merged.WriteId);
        Assert.Equal(initChange.RowAction, merged.RowAction);
    }

    [Fact]
    public void Merge_ReturnsExistingChange_WhenExistingAndIncomingReadTxIdsMatch()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer initBuffer = CreateBuffer(AddressesDummyData, addressesTable);
        long initTxId = 0;
        Change initChange = Change.Initial(initBuffer, initTxId);

        ValueBuffer buffer = CreateBuffer([1, "address", "address_1", "city"], addressesTable);
        Change incoming = initChange.Update(buffer, initTxId);

        // Act
        Change? merged = Merger.Merge(initChange, incoming);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(initChange, merged);
    }

    [Fact]
    public void Merge_ReturnsMergedChange_WhenExistingChangeWasNotWrittenToDataSource()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer initBuffer = CreateBuffer(AddressesDummyData, addressesTable);
        long initTxId = 0;
        Change initChange = Change.NewChange(initBuffer, initTxId);

        long txId = 1;
        ValueBuffer buffer = CreateBuffer([1, "address", "address_1", "city"], addressesTable);
        Change incoming = initChange.Update(buffer, txId);

        // Act
        Change? merged = Merger.Merge(initChange, incoming);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(incoming.Record, merged.Record);
        Assert.Equal(initChange.ReadId, merged.ReadId);
        Assert.Equal(incoming.WriteId, merged.WriteId);
        Assert.Equal(initChange.RowAction, merged.RowAction);
    }

    [Fact]
    public void Merge_ReturnsMergedChange_WhenExistingChangeWasWrittenToDataSource()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer initBuffer = CreateBuffer(AddressesDummyData, addressesTable);
        long initTxId = 0;
        Change initChange = Change.Initial(initBuffer, initTxId);

        long txId = 1;
        ValueBuffer buffer = CreateBuffer([1, "address", "address_1", "city"], addressesTable);
        Change incoming = initChange.Update(buffer, txId);

        // Act
        Change? merged = Merger.Merge(initChange, incoming);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(incoming.Record, merged.Record);
        Assert.Equal(initChange.ReadId, merged.ReadId);
        Assert.Equal(incoming.WriteId, merged.WriteId);
        Assert.Equal(incoming.RowAction, merged.RowAction);
    }

    [Fact]
    public void Merge_ReturnsNull_WhenNewChangeDeletesExistingChangeThatWasNotWrittenToDataSource()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer initBuffer = CreateBuffer(AddressesDummyData, addressesTable);
        long initTxId = 0;
        Change initChange = Change.NewChange(initBuffer, initTxId);

        long txId = 1;
        Change incoming = initChange.Delete(initBuffer, txId);

        // Act
        Change? merged = Merger.Merge(initChange, incoming);

        // Assert
        Assert.Null(merged);
    }

    [Fact]
    public void Merge_ThrowsTransactionMismatchException_WhenExistingAndIncomingReadTxIdsDoNotMatch()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer initBuffer = CreateBuffer(AddressesDummyData, addressesTable);
        long initTxId = 1;
        Change initChange = Change.Initial(initBuffer, initTxId);

        long txId = 0;
        ValueBuffer buffer = CreateBuffer([1, "address", "address_1", "city"], addressesTable);
        Change incoming = Change.Initial(buffer, txId);

        // Act
        Exception? exception = Record.Exception(() => _ = Merger.Merge(initChange, incoming));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConcurrencyConflictException>(exception);
    }
}
