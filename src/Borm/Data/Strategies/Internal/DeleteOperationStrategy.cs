using Borm.Data.Storage;
using Borm.Model.Metadata;

namespace Borm.Data.Strategies.Internal;

internal sealed class DeleteOperationStrategy : IDataOperationStrategy
{
    private readonly IRecordPreProcessor _preProcessor;
    private readonly IReferentialActionExecutor _executor;

    public DeleteOperationStrategy(
        IRecordPreProcessor preProcessor,
        IReferentialActionExecutor executor
    )
    {
        _preProcessor = preProcessor;
        _executor = executor;
    }

    public TransactionOperation Create(object entity, ITable table) =>
        (txId, affected) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            IEntityMetadata metadata = table.Metadata;
            IValueBuffer record = metadata.Conversion.ToValueBuffer(entity);

            IValueBuffer processed = _preProcessor.Process(record, txId, out _);

            table.Delete(processed, txId);
            affected.Add(table);

            ISet<ITable> affectedChildren = _executor.Run(table, processed.PrimaryKey, txId);
            affected.UnionWith(affectedChildren);
        };
}