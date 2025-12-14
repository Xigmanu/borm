using Borm.Data;
using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Data.Strategies.Internal;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Strategies;

public sealed class UpdateOperationStrategyTest
{
    private readonly ITableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Create_ReturnsWorkingDelegate()
    {
        // Arrange
        ITable table = _graph[typeof(PersonEntity)]!;
        ITable parent = _graph[typeof(AddressEntity)]!;
        AddressEntity address = new(1, "address", null, "city");
        PersonEntity person = new(1, "name", 42.42, address);
        string otherName = "otherName";

        ResolvedForeignKey expectedKey = new(parent, address.Id, address, true, false);
        IValueBuffer processed0 = CreateBuffer(
            MapValuesToColumns(
                [person.Id, otherName, person.Salary, address.Id],
                table.Metadata.Columns
            )
        );
        IValueBuffer processed1 = CreateBuffer(
            MapValuesToColumns(
                [address.Id, address.Address, DBNull.Value, address.City],
                parent.Metadata.Columns
            )
        );

        UpdateOperationStrategy strategy = new(
            new TestRecordPreProcessor([processed0, processed1], expectedKey)
        );
        HashSet<ITable> affected = [];
        long txId = 0;

        parent.Insert(processed1, txId);
        table.Insert(processed0, txId);

        // Act
        TransactionOperation operation = strategy.Create(person, table);
        operation(txId, affected);

        // Assert
        bool pExists = table.Tracker.TryGetChange(person.Id, txId, out IChange? personChange);
        bool aExists = parent.Tracker.TryGetChange(address.Id, txId, out IChange? addressChange);

        Assert.True(pExists);
        Assert.NotNull(personChange);
        Assert.Equal(processed0.PrimaryKey, personChange.Record.PrimaryKey);
        IColumnMetadata column = table.Metadata.Columns.First(c => c.Name == "name");
        Assert.Equal(otherName, processed0[column]);
        Assert.True(aExists);
        Assert.NotNull(addressChange);
        Assert.Equal(processed1.PrimaryKey, addressChange.Record.PrimaryKey);
        Assert.Single(affected);
    }

    [Fact]
    public void Create_ThrowsArgumentNullException_WithNullEntity()
    {
        // Arrange
        ITable table = _graph[typeof(PersonEntity)]!;
        ITable parent = _graph[typeof(AddressEntity)]!;
        AddressEntity address = new(1, "address", null, "city");

        ResolvedForeignKey expectedKey = new(parent, address.Id, address, true, false);
        IValueBuffer processed = CreateBuffer(
            MapValuesToColumns([1, "name", 42.42, 1], table.Metadata.Columns)
        );

        UpdateOperationStrategy strategy = new(
            new TestRecordPreProcessor([processed], expectedKey)
        );

        // Act
        TransactionOperation operation = strategy.Create(null!, table);
        Exception? exception = Record.Exception(() => operation(0, []));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Create_ThrowsRecordNotFoundException_WhenParentRecordDoesNotExist()
    {
        // Arrange
        ITable table = _graph[typeof(PersonEntity)]!;
        ITable parent = _graph[typeof(AddressEntity)]!;
        AddressEntity address = new(1, "address", null, "city");
        PersonEntity person = new(1, "name", 42.42, address);

        ResolvedForeignKey expectedKey = new(parent, -1, address, true, false);
        IValueBuffer processed0 = CreateBuffer(
            MapValuesToColumns(
                [person.Id, person.Name, person.Salary, address.Id],
                table.Metadata.Columns
            )
        );
        IValueBuffer processed1 = CreateBuffer(
            MapValuesToColumns(
                [address.Id, address.Address, DBNull.Value, address.City],
                parent.Metadata.Columns
            )
        );

        UpdateOperationStrategy strategy = new(
            new TestRecordPreProcessor([processed0, processed1], expectedKey)
        );
        HashSet<ITable> affected = [];
        long txId = 0;

        parent.Insert(processed1, txId);
        table.Insert(processed0, txId);

        // Act
        TransactionOperation operation = strategy.Create(person, table);
        Exception? exception = Record.Exception(() => operation(txId, affected));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<RecordNotFoundException>(exception);
        Assert.Equal(Strings.RowNotFound(parent.Name, -1), exception.Message);
    }

    private sealed class TestRecordPreProcessor : IRecordPreProcessor
    {
        private readonly IValueBuffer[] _nested;
        private readonly ResolvedForeignKey _resolvedKey;
        private int _counter;

        public TestRecordPreProcessor(IValueBuffer[] nested, ResolvedForeignKey resolvedKey)
        {
            _nested = nested;
            _resolvedKey = resolvedKey;
        }

        public IValueBuffer Process(
            IValueBuffer record,
            long txId,
            out IEnumerable<ResolvedForeignKey> keys
        )
        {
            ResolvedForeignKey key = _resolvedKey;
            if (_counter >= 1)
            {
                key = key with { ChangeExists = true };
            }

            keys = [key];
            return _nested[_counter++];
        }
    }
}