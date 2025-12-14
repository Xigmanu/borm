using Borm.Data;
using Borm.Data.Storage;
using Borm.Data.Strategies.Internal;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Strategies;

public sealed class DeleteOperationStrategyTest
{
    private readonly ITableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Create_BuildsRunnableDelegate()
    {
        // Arrange
        DeleteOperationStrategy strategy = new(
            new TestRecordPreProcessor(),
            new TestReferentialActionExecutor()
        );
        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, table.Metadata.Columns)
        );
        table.Insert(buffer, 0);
        AddressEntity entity = new(1, "address", "address_1", "city");
        HashSet<ITable> affected = [];

        // Act
        TransactionOperation operation = strategy.Create(entity, table);
        Exception? exception = Record.Exception(() => operation(0, affected));

        // Assert
        Assert.Null(exception);
        Assert.Empty(table.Tracker.Changes);
        Assert.Single(affected);
    }

    [Fact]
    public void Create_ThrowsArgumentNullException_WhenEntityObjIsNull()
    {
        // Arrange
        DeleteOperationStrategy strategy = new(
            new TestRecordPreProcessor(),
            new TestReferentialActionExecutor()
        );
        ITable table = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, table.Metadata.Columns)
        );
        table.Insert(buffer, 0);

        // Act
        TransactionOperation operation = strategy.Create(null!, table);
        Exception? exception = Record.Exception(() => operation(0, []));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    private sealed class TestRecordPreProcessor : IRecordPreProcessor
    {
        public IValueBuffer Process(
            IValueBuffer record,
            long txId,
            out IEnumerable<ResolvedForeignKey> keys
        )
        {
            keys = [];
            return record;
        }
    }

    private sealed class TestReferentialActionExecutor : IReferentialActionExecutor
    {
        public ISet<ITable> Run(ITable table, object parentPk, long txId)
        {
            return new HashSet<ITable>();
        }
    }
}