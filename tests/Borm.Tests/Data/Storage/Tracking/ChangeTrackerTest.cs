using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

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
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;
        Change incoming = Change.NewChange(buffer, txId);
        tracker.PendChange(incoming);

        // Act
        tracker.AcceptPendingChanges(txId);

        // Assert
        IEnumerable<Change> changes = tracker.Changes;
        Assert.Single(changes);
        Change actual = changes.First();

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
        IEnumerable<Change> changes = tracker.Changes;
        Assert.Empty(changes);
    }

    [Fact]
    public void PendChange_PendsIncomingChange_WithNoPendingConflict()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeTracker tracker = new();
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;
        Change incoming = Change.NewChange(buffer, txId);

        // Act
        tracker.PendChange(incoming);

        // Assert
        bool exists = tracker.TryGetChange(buffer.PrimaryKey, txId, out Change? actual);
        Assert.True(exists);
        Assert.Equal(incoming, actual);
    }

    [Fact]
    public void PendChange_PendsIncomingChange_WithPendingConflict()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeTracker tracker = new();
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;
        Change incoming0 = Change.NewChange(buffer, txId);
        Change incoming1 = incoming0.Update(buffer, txId);

        // Act
        tracker.PendChange(incoming0);
        tracker.PendChange(incoming1);

        // Assert
        bool exists = tracker.TryGetChange(buffer.PrimaryKey, txId, out Change? actual);
        Assert.True(exists);
        Assert.NotNull(actual);
        Assert.Equal(incoming1.Record, actual.Record);
    }
}
