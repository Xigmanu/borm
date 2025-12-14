using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage.Tracking;

public sealed class ChangeMergerTest
{
    private readonly ITableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void CommitMerge_ReturnsCommittedMergedChange()
    {
        // Arrange
        ITable addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        const long initTxId = 0;
        IChange initChange = ChangeFactory.NewChange(initBuffer, initTxId);

        const long txId = 1;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata.Columns)
        );
        IChange incoming = ChangeFactory.Update(initChange, buffer, txId);
        Merger merger = new();

        // Act
        IChange? merged = merger.Merge(initChange, incoming, MergeMode.Commit);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(incoming.Record, merged.Record);
        Assert.Equal(incoming.WriteId, merged.ReadId);
        Assert.Equal(incoming.WriteId, merged.WriteId);
        Assert.Equal(initChange.Operation, merged.Operation);
    }

    [Fact]
    public void Merge_ReturnsExistingChange_WhenExistingAndIncomingReadTxIdsMatch()
    {
        // Arrange
        ITable addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        const long initTxId = 0;
        IChange initChange = ChangeFactory.Initial(initBuffer, initTxId);

        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata.Columns)
        );
        IChange incoming = ChangeFactory.Update(initChange, buffer, initTxId);
        Merger merger = new();

        // Act
        IChange? merged = merger.Merge(initChange, incoming, MergeMode.Normal);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(initChange, merged);
    }

    [Fact]
    public void Merge_ReturnsMergedChange_WhenExistingChangeWasNotWrittenToDataSource()
    {
        // Arrange
        ITable addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        const long initTxId = 0;
        IChange initChange = ChangeFactory.NewChange(initBuffer, initTxId);

        const long txId = 1;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata.Columns)
        );
        IChange incoming = ChangeFactory.Update(initChange, buffer, txId);
        Merger merger = new();

        // Act
        IChange? merged = merger.Merge(initChange, incoming, MergeMode.Normal);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(incoming.Record, merged.Record);
        Assert.Equal(initChange.ReadId, merged.ReadId);
        Assert.Equal(incoming.WriteId, merged.WriteId);
        Assert.Equal(initChange.Operation, merged.Operation);
    }

    [Fact]
    public void Merge_ReturnsMergedChange_WhenExistingChangeWasWrittenToDataSource()
    {
        // Arrange
        ITable addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        const long initTxId = 0;
        IChange initChange = ChangeFactory.Initial(initBuffer, initTxId);

        const long txId = 1;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata.Columns)
        );
        IChange incoming = ChangeFactory.Update(initChange, buffer, txId);
        Merger merger = new();

        // Act
        IChange? merged = merger.Merge(initChange, incoming, MergeMode.Normal);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(incoming.Record, merged.Record);
        Assert.Equal(initChange.ReadId, merged.ReadId);
        Assert.Equal(incoming.WriteId, merged.WriteId);
        Assert.Equal(incoming.Operation, merged.Operation);
    }

    [Fact]
    public void Merge_ReturnsNull_WhenNewChangeDeletesExistingChangeThatWasNotWrittenToDataSource()
    {
        // Arrange
        ITable addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        const long initTxId = 0;
        IChange initChange = ChangeFactory.NewChange(initBuffer, initTxId);

        const long txId = 1;
        IChange incoming = ChangeFactory.Delete(initChange, initBuffer, txId);
        Merger merger = new();

        // Act
        IChange? merged = merger.Merge(initChange, incoming, MergeMode.Normal);

        // Assert
        Assert.Null(merged);
    }

    [Fact]
    public void Merge_ThrowsTransactionMismatchException_WhenExistingAndIncomingReadTxIdsDoNotMatch()
    {
        // Arrange
        ITable addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        const long initTxId = 1;
        IChange initChange = ChangeFactory.Initial(initBuffer, initTxId);

        const long txId = 0;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata.Columns)
        );
        IChange incoming = ChangeFactory.Initial(buffer, txId);
        Merger merger = new();

        // Act
        Exception? exception = Record.Exception(() => _ = merger.Merge(initChange, incoming, MergeMode.Normal)
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConcurrencyConflictException>(exception);
    }
}