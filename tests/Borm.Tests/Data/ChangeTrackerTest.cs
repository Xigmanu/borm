using Borm.Data;
using static Borm.Tests.Mocks.ValueBufferMockHelper;
using static Borm.Tests.Mocks.TableMocks;

namespace Borm.Tests.Data;

public sealed class ChangeTrackerTest
{
    [Fact]
    public void AcceptPendingChanges_ConvertsPendingChangeToNormalChange()
    {
        // Arrange
        Table addressesTable = CreateAddressesTable();
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

        Assert.Equal(incoming.Buffer, actual.Buffer);
        Assert.Equal(incoming.WriteTxId, actual.WriteTxId);
        Assert.Equal(actual.WriteTxId, actual.ReadTxId);
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
        Table addressesTable = CreateAddressesTable();
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
        Table addressesTable = CreateAddressesTable();
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
        Assert.Equal(incoming1.Buffer, actual.Buffer);
    }
}
