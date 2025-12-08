using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage.Tracking;

public sealed class ChangeSetTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();
    private readonly IMerger _testMerger = new TestMerger();

    [Fact]
    public void Add_AddsChange_WithMatchingPrimaryKeys()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        IChange initial = ChangeFactory.Initial(buffer, -1);

        ChangeSet changes = new(_testMerger) { initial };

        long txId = 0;
        IChange incoming = ChangeFactory.Update(initial, buffer, txId);

        // Act
        changes.Add(incoming);

        // Assert
        Assert.Single(changes);
        IChange actual = changes.First();
        Assert.Equal(incoming.WriteId, actual.WriteId);
    }

    [Fact]
    public void Add_AddsChange_WithMatchingPrimaryKeysAndNullMerge()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        const long txId = 0;
        IChange newChange = ChangeFactory.NewChange(buffer, txId);

        ChangeSet changes = new(new Merger()) { newChange };

        IChange incoming = ChangeFactory.Delete(newChange, buffer, txId);

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
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        const long txId = 0;
        IChange incoming = ChangeFactory.NewChange(buffer, txId);

        // Act
        ChangeSet changes = new(_testMerger) { incoming };

        // Assert
        Assert.Single(changes);
        IChange actual = changes.First();
        Assert.Equal(incoming, actual);
    }

    [Fact]
    public void MarkAsWritten_MarksAllNonDeleteChangesAsWritten()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        const long txId = 0;
        IChange incoming = ChangeFactory.NewChange(buffer, txId);

        ChangeSet changes = new(_testMerger) { incoming };

        // Act
        changes.MarkAsWritten();

        // Assert
        Assert.Single(changes);
        IChange actual = changes.First();
        Assert.True(actual.IsWrittenToDataSource);
    }

    [Fact]
    public void MarkAsWritten_RemovesAllDeleteChanges()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        IChange initial = ChangeFactory.Initial(buffer, -1);

        ChangeSet changes = new(_testMerger) { initial };

        const long txId = 0;
        IChange incoming = ChangeFactory.Delete(initial, buffer, txId);
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
        ChangeSet existing = new(_testMerger);
        IValueBuffer initial = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        IChange initialChange = ChangeFactory.Initial(initial, -1);
        existing.Add(initialChange);

        ChangeSet incoming = new(_testMerger);
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        IChange updateChange = ChangeFactory.Update(initialChange, buffer, 0);
        incoming.Add(updateChange);

        // Act
        ChangeSet merged = existing.MergeWith(incoming);

        // Assert
        Assert.Single(merged);
        IChange actual = merged.First();
        Assert.Equal(updateChange.Record, actual.Record);
        Assert.Equal(updateChange.WriteId, actual.WriteId);
    }

    [Fact]
    public void Merge_MergesExistingWithIncoming_WithNoConflict()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeSet existing = new(_testMerger);
        IValueBuffer initial = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        IChange initialChange = ChangeFactory.Initial(initial, -1);
        existing.Add(initialChange);

        ChangeSet incoming = new(_testMerger);
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(
                [2, "address", DBNull.Value, "city"],
                addressesTable.Metadata.Columns
            )
        );
        IChange newChange = ChangeFactory.NewChange(buffer, 0);
        incoming.Add(newChange);

        // Act
        ChangeSet merged = existing.MergeWith(incoming);

        // Assert
        Assert.Equal(2, merged.Count);
        IChange actualInitial = merged.First();
        Assert.Equal(initialChange, actualInitial);
        IChange actualNew = merged.ElementAt(1);
        Assert.Equal(newChange, actualNew);
    }

    [Fact]
    public void Merge_ThrowsInvalidOperationException_WhenNonExistingRowIsModified()
    {
        // Arrange
        Table addressesTable = _graph[typeof(AddressEntity)]!;
        ChangeSet existing = new(_testMerger);
        IValueBuffer initial = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        IChange initialChange = ChangeFactory.Initial(initial, -1);
        existing.Add(initialChange);

        ChangeSet incoming = new(_testMerger);
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(
                [2, "address", DBNull.Value, "city"],
                addressesTable.Metadata.Columns
            )
        );
        IChange newChange = ChangeFactory.Update(initialChange, buffer, 0);
        incoming.Add(newChange);

        // Act
        Exception? exception = Record.Exception(() => _ = existing.MergeWith(incoming));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    private sealed class TestMerger : IMerger
    {
        public IChange Merge(IChange existing, IChange incoming, MergeMode mode)
        {
            return incoming;
        }
    }
}