using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage.Tracking;

public sealed class ChangeTrackerTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void AcceptPendingChanges_ConvertsPendingChangeToNormalChange()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeTracker tracker = new();
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long txId = 0;
        IChange incoming = ChangeFactory.NewChange(buffer, txId);
        tracker.PendChange(incoming);

        // Act
        tracker.AcceptPendingChanges(txId);

        // Assert
        IEnumerable<IChange> changes = tracker.Changes;
        Assert.Single(changes);
        IChange actual = changes.First();

        Assert.Equal(incoming.Record, actual.Record);
        Assert.Equal(incoming.WriteId, actual.WriteId);
        Assert.Equal(actual.WriteId, actual.ReadId);
    }

    [Fact]
    public void AcceptPendingChanges_DoesNothing_WhenNoPendingChangesExistForTransaction()
    {
        // Arrange
        ChangeTracker tracker = new();

        // Act
        tracker.AcceptPendingChanges(0);

        // Assert
        IEnumerable<IChange> changes = tracker.Changes;
        Assert.Empty(changes);
    }

    [Fact]
    public void PendChange_PendsIncomingChange_WithNoPendingConflict()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeTracker tracker = new();
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long txId = 0;
        IChange incoming = ChangeFactory.NewChange(buffer, txId);

        // Act
        tracker.PendChange(incoming);

        // Assert
        bool exists = tracker.TryGetChange(buffer.PrimaryKey, txId, out IChange? actual);
        Assert.True(exists);
        Assert.Equal(incoming, actual);
    }

    [Fact]
    public void PendChange_PendsIncomingChange_WithPendingConflict()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeTracker tracker = new();
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata)
        );
        long txId = 0;
        IChange incoming0 = ChangeFactory.NewChange(buffer, txId);
        IChange incoming1 = ChangeFactory.Update(incoming0, buffer, txId);

        // Act
        tracker.PendChange(incoming0);
        tracker.PendChange(incoming1);

        // Assert
        bool exists = tracker.TryGetChange(buffer.PrimaryKey, txId, out IChange? actual);
        Assert.True(exists);
        Assert.NotNull(actual);
        Assert.Equal(incoming1.Record, actual.Record);
    }
}
