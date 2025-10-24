using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data.Storage.Tracking;

public sealed class ChangeTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

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
        Assert.Equal(txId, actual.WriteId);
        Assert.Equal(initTxId, actual.ReadId);
        Assert.Equal(RowAction.Delete, actual.RowAction);
        Assert.True(actual.IsWrittenToDataSource);
        Assert.Equal(buffer, actual.Record);
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
        Assert.Equal(txId, change.WriteId);
        Assert.Equal(change.ReadId, change.WriteId);
        Assert.Equal(RowAction.None, change.RowAction);
        Assert.True(change.IsWrittenToDataSource);
        Assert.Equal(buffer, change.Record);
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
        Assert.Equal(txId, change.WriteId);
        Assert.Equal(change.WriteId, change.ReadId);
        Assert.Equal(RowAction.None, change.RowAction);
        Assert.True(change.IsWrittenToDataSource);
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
        Assert.Equal(txId, change.WriteId);
        Assert.Equal(change.ReadId, change.WriteId);
        Assert.Equal(RowAction.Insert, change.RowAction);
        Assert.False(change.IsWrittenToDataSource);
        Assert.Equal(buffer, change.Record);
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
        Assert.Equal(txId, actual.WriteId);
        Assert.Equal(initTxId, actual.ReadId);
        Assert.Equal(RowAction.Update, actual.RowAction);
        Assert.False(actual.IsWrittenToDataSource);
        Assert.Equal(buffer, actual.Record);
    }
}
