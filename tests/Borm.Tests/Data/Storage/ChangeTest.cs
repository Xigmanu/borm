using Borm.Data.Storage;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data.Storage;

public sealed class ChangeTest
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
        Change? merged = initChange.CommitMerge(incoming);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(incoming.Buffer, merged.Buffer);
        Assert.Equal(incoming.WriteTxId, merged.ReadTxId);
        Assert.Equal(incoming.WriteTxId, merged.WriteTxId);
        Assert.Equal(initChange.RowAction, merged.RowAction);
    }

    [Fact]
    public void Delete_ReturnsDeleteChange()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer initBuffer = CreateBuffer(AddressesDummyData, addressesTable);
        long initTxId = 0;
        Change change = Change.Initial(initBuffer, initTxId);

        ValueBuffer buffer = CreateBuffer([1, "address", "address_1", "city"], addressesTable);
        long txId = 1;

        // Act
        Change actual = change.Delete(buffer, txId);

        // Assert
        Assert.Equal(txId, actual.WriteTxId);
        Assert.Equal(initTxId, actual.ReadTxId);
        Assert.Equal(RowAction.Delete, actual.RowAction);
        Assert.True(actual.IsWrittenToDb);
        Assert.Equal(buffer, actual.Buffer);
    }

    [Fact]
    public void InitChange_ReturnsInitialChange()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;

        // Act
        Change change = Change.Initial(buffer, txId);

        // Assert
        Assert.Equal(txId, change.WriteTxId);
        Assert.Equal(change.ReadTxId, change.WriteTxId);
        Assert.Equal(RowAction.None, change.RowAction);
        Assert.True(change.IsWrittenToDb);
        Assert.Equal(buffer, change.Buffer);
    }

    [Fact]
    public void MarkAsWritten_MarksChangeAsWrittenToDataSource()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;
        Change change = Change.Initial(buffer, txId);

        // Act
        change.MarkAsWritten();

        // Assert
        Assert.Equal(txId, change.WriteTxId);
        Assert.Equal(change.WriteTxId, change.ReadTxId);
        Assert.Equal(RowAction.None, change.RowAction);
        Assert.True(change.IsWrittenToDb);
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
        Change? merged = initChange.Merge(incoming);

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
        Change? merged = initChange.Merge(incoming);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(incoming.Buffer, merged.Buffer);
        Assert.Equal(initChange.ReadTxId, merged.ReadTxId);
        Assert.Equal(incoming.WriteTxId, merged.WriteTxId);
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
        Change? merged = initChange.Merge(incoming);

        // Assert
        Assert.NotNull(merged);
        Assert.Equal(incoming.Buffer, merged.Buffer);
        Assert.Equal(initChange.ReadTxId, merged.ReadTxId);
        Assert.Equal(incoming.WriteTxId, merged.WriteTxId);
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
        Change? merged = initChange.Merge(incoming);

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
        Exception? exception = Record.Exception(() => _ = initChange.Merge(incoming));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConcurrencyConflictException>(exception);
    }

    [Fact]
    public void NewChange_ReturnsInsertChange()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;

        // Act
        Change change = Change.NewChange(buffer, txId);

        // Assert
        Assert.Equal(txId, change.WriteTxId);
        Assert.Equal(change.ReadTxId, change.WriteTxId);
        Assert.Equal(RowAction.Insert, change.RowAction);
        Assert.False(change.IsWrittenToDb);
        Assert.Equal(buffer, change.Buffer);
    }

    [Fact]
    public void Update_ReturnsUpdateChange()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer initBuffer = CreateBuffer(AddressesDummyData, addressesTable);
        long initTxId = 0;
        Change change = Change.NewChange(initBuffer, initTxId);

        ValueBuffer buffer = CreateBuffer([1, "address", "address_1", "city"], addressesTable);
        long txId = 1;

        // Act
        Change actual = change.Update(buffer, txId);

        // Assert
        Assert.Equal(txId, actual.WriteTxId);
        Assert.Equal(initTxId, actual.ReadTxId);
        Assert.Equal(RowAction.Update, actual.RowAction);
        Assert.False(actual.IsWrittenToDb);
        Assert.Equal(buffer, actual.Buffer);
    }
}
