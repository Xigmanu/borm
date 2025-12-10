using Borm.Data;
using Borm.Data.Storage;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Data;

public sealed class TransactionOperationFactoryTest
{
    private readonly ITableGraph _graph = TableGraphMock.Create();

    [Theory]
    [InlineData(nameof(OperationKind.None), true)]
    [InlineData(nameof(OperationKind.Insert), false)]
    [InlineData(nameof(OperationKind.Update), false)]
    [InlineData(nameof(OperationKind.Delete), false)]
    public void Create_ThrowsNotSupportedException_WhenOperationKindIsNotSupported(string operationStr, bool throws)
    {
        // Arrange
        object entity = new AddressEntity(1, "address", null, "city");
        ITable table = _graph[typeof(AddressEntity)]!;
        OperationKind operation = MapToEnum(operationStr);
        TransactionOperationFactory factory = new(new TestRecordPreProcessor(), new TestReferentialActionExecutor());

        // Act
        Exception? exception = Record.Exception(() => _ = factory.Create(entity, table, operation));

        // Assert
        if (throws)
        {
            Assert.NotNull(exception);
            Assert.IsType<NotSupportedException>(exception);
        }
        else
        {
            Assert.Null(exception);
        }
    }

    private static OperationKind MapToEnum(string operation) =>
        operation switch
        {
            nameof(OperationKind.Insert) => OperationKind.Insert,
            nameof(OperationKind.Update) => OperationKind.Update,
            nameof(OperationKind.Delete) => OperationKind.Delete,
            _ => OperationKind.None
        };

    private sealed class TestRecordPreProcessor : IRecordPreProcessor
    {
        public IValueBuffer Process(IValueBuffer record, long txId, out IEnumerable<ResolvedForeignKey> keys)
        {
            keys = new List<ResolvedForeignKey>();
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