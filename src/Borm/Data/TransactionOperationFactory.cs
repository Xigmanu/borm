using Borm.Data.Storage;
using Borm.Data.Strategies;
using Borm.Data.Strategies.Internal;

namespace Borm.Data;

internal sealed class TransactionOperationFactory
{
    private readonly IReferentialActionExecutor _executor;
    private readonly IRecordPreProcessor _preProcessor;

    public TransactionOperationFactory(
        IRecordPreProcessor preProcessor,
        IReferentialActionExecutor executor
    )
    {
        _preProcessor = preProcessor;
        _executor = executor;
    }

    public TransactionOperation Create(object entity, ITable table, OperationKind operation)
    {
        IDataOperationStrategy strategy = operation switch
        {
            OperationKind.Insert => new InsertOperationStrategy(_preProcessor),
            OperationKind.Update => new UpdateOperationStrategy(_preProcessor),
            OperationKind.Delete => new DeleteOperationStrategy(_preProcessor, _executor),
            OperationKind.None => throw new NotSupportedException(),
            _ => throw new NotSupportedException()
        };

        return strategy.Create(entity, table);
    }
}