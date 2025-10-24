using System.Collections.Immutable;
using System.Data;
using Borm.Data.Sql;
using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data.Storage;

public sealed class TableTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Delete_PendsRecordDeletion()
    {
        // Arrange
        long initTxId = -1;
        long txId = 0;
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        table.Tracker.PendChange(Change.Initial(buffer, initTxId));
        table.Tracker.AcceptPendingChanges(initTxId);

        // Act
        table.Delete(buffer, txId);
        table.Tracker.AcceptPendingChanges(txId);

        // Assert
        ImmutableList<Change> changes = table.Tracker.Changes;

        Assert.Single(changes);
        Assert.Equal(RowAction.Delete, changes[0].RowAction);
        Assert.Equal(txId, changes[0].WriteId);
        Assert.Equal(buffer, changes[0].Record);
    }

    [Fact]
    public void Delete_ThrowsRowNotFoundException_WhenTargetRecordDoesNotExist()
    {
        // Arrange
        long txId = 0;
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);

        // Act
        Exception? exception = Record.Exception(() => table.Delete(buffer, txId));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<RecordNotFoundException>(exception);
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenTablesAreNotEqual()
    {
        // Arrange
        Table first = _graph[typeof(AddressEntity)]!;
        Table second = _graph[typeof(PersonEntity)]!;

        // Act
        bool equal = first.Equals(second);

        // Assert
        Assert.False(equal);
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenTablesAreEqual()
    {
        // Arrange
        EntityMetadata metadata = _graph[typeof(AddressEntity)]!.Metadata;
        Table first = new(metadata);
        Table second = new(metadata);

        // Act
        bool equal = first.Equals(second);

        // Assert
        Assert.True(equal);
    }

    [Fact]
    public void Insert_PendsRecordInsertion()
    {
        // Arrange
        long txId = 0;
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);

        // Act
        table.Insert(buffer, txId);
        table.Tracker.AcceptPendingChanges(txId);

        // Assert
        ImmutableList<Change> changes = table.Tracker.Changes;

        Assert.Single(changes);
        Assert.Equal(RowAction.Insert, changes[0].RowAction);
        Assert.Equal(buffer, changes[0].Record);
    }

    [Fact]
    public void Insert_ThrowsConstraintException_WhenPrimaryKeysCollide()
    {
        // Arrange
        long initTxId = -1;
        long txId = 0;
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        table.Tracker.PendChange(Change.Initial(buffer, initTxId));
        table.Tracker.AcceptPendingChanges(initTxId);

        // Act
        Exception? exception = Record.Exception(() => table.Insert(buffer, txId));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConstraintException>(exception);

        Assert.Single(table.Tracker.Changes);
    }

    [Fact]
    public void Insert_ThrowsConstraintException_WhenIncomingBufferViolatesConstraints()
    {
        // Arrange
        long txId = 0;
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer([1, DBNull.Value, DBNull.Value, "city"], table);

        // Act
        Exception? exception = Record.Exception(() => table.Insert(buffer, txId));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConstraintException>(exception);

        Assert.Empty(table.Tracker.Changes);
    }

    [Fact]
    public void Load_DoesNothing_WhenResultSetIsEmpty()
    {
        // Arrange
        ResultSet resultSet = new();
        Table table = _graph[typeof(AddressEntity)]!;
        long initTxId = -1;

        // Act
        table.Load(resultSet, initTxId);
        table.Tracker.AcceptPendingChanges(initTxId);

        // Assert
        Assert.Empty(table.Tracker.Changes);
    }

    [Fact]
    public void Load_PendsInitialChanges_WithResultSet()
    {
        // Arrange
        ResultSet resultSet = new();
        Dictionary<string, object> row = new()
        {
            ["id"] = 1,
            ["address"] = "address",
            ["address_1"] = DBNull.Value,
            ["city"] = "city",
        };
        resultSet.AddRow(row);
        long initTxId = -1;
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);

        // Act
        table.Load(resultSet, initTxId);
        table.Tracker.AcceptPendingChanges(initTxId);

        // Assert
        Assert.Single(table.Tracker.Changes);

        Change change = table.Tracker.Changes[0];
        Assert.Equal(RowAction.None, change.RowAction);
        Assert.True(change.IsWrittenToDataSource);
        Assert.Equal(buffer, change.Record);
    }

    [Fact]
    public void Update_PendsRecordUpdate()
    {
        // Arrange
        long initTxId = -1;
        long txId = 0;
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        ValueBuffer bufferUpdate = CreateBuffer([1, "address", "address_1", "city"], table);
        table.Tracker.PendChange(Change.Initial(buffer, initTxId));
        table.Tracker.AcceptPendingChanges(initTxId);

        // Act
        table.Update(bufferUpdate, txId);
        table.Tracker.AcceptPendingChanges(txId);

        // Assert
        ImmutableList<Change> changes = table.Tracker.Changes;
        Assert.Single(changes);
        Assert.Equal(RowAction.Update, changes[0].RowAction);
        Assert.Equal(bufferUpdate, changes[0].Record);
    }

    [Fact]
    public void Update_ThrowsConstraintException_WhenIncomingBufferViolatesConstraints()
    {
        // Arrange
        long initTxId = -1;
        long txId = 0;
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        ValueBuffer bufferUpdate = CreateBuffer([1, DBNull.Value, "address_1", "city"], table);
        table.Tracker.PendChange(Change.Initial(buffer, initTxId));
        table.Tracker.AcceptPendingChanges(initTxId);

        // Act
        Exception? exception = Record.Exception(() => table.Update(bufferUpdate, txId));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConstraintException>(exception);

        ImmutableList<Change> changes = table.Tracker.Changes;
        Assert.Single(changes);
        Assert.Equal(RowAction.None, changes[0].RowAction);
        Assert.Equal(buffer, changes[0].Record);
    }

    [Fact]
    public void Update_ThrowsRowNotFoundException_WhenTargetRecordDoesNotExist()
    {
        // Arrange
        long txId = 0;
        Table table = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);

        // Act
        Exception? exception = Record.Exception(() => table.Update(buffer, txId));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<RecordNotFoundException>(exception);
    }
}
