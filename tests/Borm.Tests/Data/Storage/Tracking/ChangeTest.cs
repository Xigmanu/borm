using Borm.Data;
using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage.Tracking;

public sealed class ChangeTest
{
    private readonly ITableGraph _graph = TableGraphMock.Create();

    public static TheoryData<object?, bool> TestData =>
        new()
        {
            { null, false },
            { string.Empty, false },
            { 2, false },
            { 1, true }
        };

    [Fact]
    public void Constructor_InitializesNewInstanceWithCorrectPropertyValues()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer record = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, table.Metadata.Columns)
        );
        long readId = 0;
        long writeId = 1;
        bool isWrittenToDataSource = false;
        OperationKind operation = OperationKind.Insert;

        // Act
        Change change = new(record, readId, writeId, isWrittenToDataSource, operation);

        // Assert
        Assert.Equal(record, change.Record);
        Assert.Equal(readId, change.ReadId);
        Assert.Equal(writeId, change.WriteId);
        Assert.Equal(isWrittenToDataSource, change.IsWrittenToDataSource);
        Assert.Equal(operation, change.Operation);
    }

    [Theory]
    [MemberData(nameof(TestData))]
    public void Equals_ChecksIfChangesAreEqual(object? primaryKey, bool check)
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer record = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, table.Metadata.Columns)
        );
        Change change = new(record, 0, 0, false, OperationKind.Insert);

        object? other = null;
        if (primaryKey is not null)
        {
            if (primaryKey.GetType() != typeof(int))
            {
                other = primaryKey;
            }
            else
            {
                IValueBuffer otherRecord = CreateBuffer(
                    MapValuesToColumns(
                        [primaryKey, "address", "address_1", "city"],
                        table.Metadata.Columns
                    )
                );
                other = new Change(otherRecord, 0, 0, false, OperationKind.Insert);
            }
        }

        // Act
        bool equal = change.Equals(other);

        // Assert
        Assert.Equal(check, equal);
    }

    [Fact]
    public void GetHashCode_ReturnsCorrectHashCode()
    {
        // Arrange
        object primaryKey = 1;

        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer record = CreateBuffer(
            MapValuesToColumns([primaryKey, "address", "address_1", "city"], table.Metadata.Columns)
        );
        Change other = new(record, 0, 0, false, OperationKind.Insert);

        int expected = primaryKey.GetHashCode();
        // Act
        int hash = other.GetHashCode();

        // Assert
        Assert.Equal(hash, expected);
    }

    [Fact]
    public void MarkAsCommittedToDataSource_SetsWrittenToDataSourceToTrueAndOperationToNone()
    {
        // Arrange
        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer record = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, table.Metadata.Columns)
        );
        long readId = 0;
        long writeId = 1;
        bool isWrittenToDataSource = false;
        OperationKind operation = OperationKind.Insert;

        Change change = new(record, readId, writeId, isWrittenToDataSource, operation);

        // Act
        IChange written = change.MarkAsCommittedToDataSource();

        // Assert
        Assert.True(written.IsWrittenToDataSource);
        Assert.Equal(OperationKind.None, written.Operation);

        Assert.Equal(change.ReadId, written.ReadId);
        Assert.Equal(change.WriteId, written.WriteId);
        Assert.Equal(change.Record, written.Record);
    }
}