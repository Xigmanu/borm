using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage.Tracking;

public sealed class ChangeMergerTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void CommitMerge_ReturnsCommittedMergedChange()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long initTxId = 0;
        IChange initChange = ChangeFactory.NewChange(initBuffer, initTxId);

        long txId = 1;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata)
        );
        IChange incoming = ChangeFactory.Update(initChange, buffer, txId);

        // Act
        IChange? merged = Merger.CommitMerge(initChange, incoming);

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
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long initTxId = 0;
        IChange initChange = ChangeFactory.Initial(initBuffer, initTxId);

        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata)
        );
        IChange incoming = ChangeFactory.Update(initChange, buffer, initTxId);

        // Act
        IChange? merged = Merger.Merge(initChange, incoming);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(initChange, merged);
    }

    [Fact]
    public void Merge_ReturnsMergedChange_WhenExistingChangeWasNotWrittenToDataSource()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long initTxId = 0;
        IChange initChange = ChangeFactory.NewChange(initBuffer, initTxId);

        long txId = 1;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata)
        );
        IChange incoming = ChangeFactory.Update(initChange, buffer, txId);

        // Act
        IChange? merged = Merger.Merge(initChange, incoming);

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
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long initTxId = 0;
        IChange initChange = ChangeFactory.Initial(initBuffer, initTxId);

        long txId = 1;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata)
        );
        IChange incoming = ChangeFactory.Update(initChange, buffer, txId);

        // Act
        IChange? merged = Merger.Merge(initChange, incoming);

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
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long initTxId = 0;
        IChange initChange = ChangeFactory.NewChange(initBuffer, initTxId);

        long txId = 1;
        IChange incoming = ChangeFactory.Delete(initChange, initBuffer, txId);

        // Act
        IChange? merged = Merger.Merge(initChange, incoming);

        // Assert
        Assert.Null(merged);
    }

    [Fact]
    public void Merge_ThrowsTransactionMismatchException_WhenExistingAndIncomingReadTxIdsDoNotMatch()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long initTxId = 1;
        IChange initChange = ChangeFactory.Initial(initBuffer, initTxId);

        long txId = 0;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata)
        );
        IChange incoming = ChangeFactory.Initial(buffer, txId);

        // Act
        Exception? exception = Record.Exception(() => _ = Merger.Merge(initChange, incoming));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConcurrencyConflictException>(exception);
    }
}
