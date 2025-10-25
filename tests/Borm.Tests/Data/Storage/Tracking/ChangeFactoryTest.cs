using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage.Tracking;

public sealed class ChangeFactoryTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Delete_ReturnsDeleteChange()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long initTxId = 0;
        IChange change = ChangeFactory.Initial(initBuffer, initTxId);

        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata)
        );
        long txId = 1;

        // Act
        IChange actual = ChangeFactory.Delete(change, buffer, txId);

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
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long txId = 0;

        // Act
        IChange change = ChangeFactory.Initial(buffer, txId);

        // Assert
        Assert.Equal(txId, change.WriteId);
        Assert.Equal(change.ReadId, change.WriteId);
        Assert.Equal(RowAction.None, change.RowAction);
        Assert.True(change.IsWrittenToDataSource);
        Assert.Equal(buffer, change.Record);
    }

    [Fact]
    public void NewChange_ReturnsInsertChange()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long txId = 0;

        // Act
        IChange change = ChangeFactory.NewChange(buffer, txId);

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
        IValueBuffer initBuffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long initTxId = 0;
        IChange change = ChangeFactory.NewChange(initBuffer, initTxId);

        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns([1, "address", "address_1", "city"], addressesTable.Metadata)
        );
        long txId = 1;

        // Act
        IChange actual = ChangeFactory.Update(change, buffer, txId);

        // Assert
        Assert.Equal(txId, actual.WriteId);
        Assert.Equal(initTxId, actual.ReadId);
        Assert.Equal(RowAction.Update, actual.RowAction);
        Assert.False(actual.IsWrittenToDataSource);
        Assert.Equal(buffer, actual.Record);
    }
}
