using Borm.Data.Storage;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data.Storage;

public sealed class ChangeSetTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Add_AddsChange_WithMatchingPrimaryKeys()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        Change initial = Change.Initial(buffer, -1);

        ChangeSet changes = [];
        changes.Add(initial);

        long txId = 0;
        Change incoming = initial.Update(buffer, txId);

        // Act
        changes.Add(incoming);

        // Assert
        Assert.Single(changes);
        Change actual = changes.First();
        Assert.Equal(incoming.WriteTxId, actual.WriteTxId);
    }

    [Fact]
    public void Add_AddsChange_WithMatchingPrimaryKeysAndNullMerge()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;
        Change newChange = Change.NewChange(buffer, txId);

        ChangeSet changes = [];
        changes.Add(newChange);

        Change incoming = newChange.Delete(buffer, txId);

        // Act
        changes.Add(incoming);

        // Assert
        Assert.Empty(changes);
    }

    [Fact]
    public void Add_AddsChange_WithNoConflict()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;
        Change incoming = Change.NewChange(buffer, txId);

        ChangeSet changes = [];

        // Act
        changes.Add(incoming);

        // Assert
        Assert.Single(changes);
        Change actual = changes.First();
        Assert.Equal(incoming, actual);
    }

    [Fact]
    public void MarkAsWritten_MarksAllNonDeleteChangesAsWritten()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;
        Change incoming = Change.NewChange(buffer, txId);

        ChangeSet changes = [];
        changes.Add(incoming);

        // Act
        changes.MarkAsWritten();

        // Assert
        Assert.Single(changes);
        Change actual = changes.First();
        Assert.True(actual.IsWrittenToDb);
    }

    [Fact]
    public void MarkAsWritten_RemovesAllDeleteChanges()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        Change initial = Change.Initial(buffer, -1);

        ChangeSet changes = [];
        changes.Add(initial);

        long txId = 0;
        Change incoming = initial.Delete(buffer, txId);
        changes.Add(incoming);

        // Act
        changes.MarkAsWritten();

        // Assert
        Assert.Empty(changes);
    }

    [Fact]
    public void Merge_MergesExistingWithIncoming_WithConflict()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeSet existing = [];
        ValueBuffer initial = CreateBuffer(AddressesDummyData, addressesTable);
        Change initialChange = Change.Initial(initial, -1);
        existing.Add(initialChange);

        ChangeSet incoming = [];
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        Change updateChange = initialChange.Update(buffer, 0);
        incoming.Add(updateChange);

        // Act
        ChangeSet merged = ChangeSet.Merge(existing, incoming);

        // Assert
        Assert.Single(merged);
        Change actual = merged.First();
        Assert.Equal(updateChange.Buffer, actual.Buffer);
        Assert.Equal(updateChange.WriteTxId, actual.WriteTxId);
    }

    [Fact]
    public void Merge_MergesExistingWithIncoming_WithConflictAndNullMerge()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeSet existing = [];
        ValueBuffer initial = CreateBuffer(AddressesDummyData, addressesTable);
        long txId = 0;
        Change initialChange = Change.NewChange(initial, txId);
        existing.Add(initialChange);

        ChangeSet incoming = [];
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, addressesTable);
        Change deleteChange = initialChange.Delete(buffer, txId);
        incoming.Add(deleteChange);

        // Act
        ChangeSet merged = ChangeSet.Merge(existing, incoming);

        // Assert
        Assert.Empty(merged);
    }

    [Fact]
    public void Merge_MergesExistingWithIncoming_WithNoConflict()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeSet existing = [];
        ValueBuffer initial = CreateBuffer(AddressesDummyData, addressesTable);
        Change initialChange = Change.Initial(initial, -1);
        existing.Add(initialChange);

        ChangeSet incoming = [];
        ValueBuffer buffer = CreateBuffer([2, "address", DBNull.Value, "city"], addressesTable);
        Change newChange = Change.NewChange(buffer, 0);
        incoming.Add(newChange);

        // Act
        ChangeSet merged = ChangeSet.Merge(existing, incoming);

        // Assert
        Assert.Equal(2, merged.Count());
        Change actualInitial = merged.First();
        Assert.Equal(initialChange, actualInitial);
        Change actualNew = merged.ElementAt(1);
        Assert.Equal(newChange, actualNew);
    }

    [Fact]
    public void Merge_ThrowsInvalidOperationException_WhenNonExistingRowIsModified()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeSet existing = [];
        ValueBuffer initial = CreateBuffer(AddressesDummyData, addressesTable);
        Change initialChange = Change.Initial(initial, -1);
        existing.Add(initialChange);

        ChangeSet incoming = [];
        ValueBuffer buffer = CreateBuffer([2, "address", DBNull.Value, "city"], addressesTable);
        Change newChange = initialChange.Update(buffer, 0);
        incoming.Add(newChange);

        // Act
        Exception? exception = Record.Exception(() => _ = ChangeSet.Merge(existing, incoming));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}
