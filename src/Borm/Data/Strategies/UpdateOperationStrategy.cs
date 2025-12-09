using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data.Strategies;

internal sealed class UpdateOperationStrategy : IDataOperationStrategy
{
    private readonly IRecordPreProcessor _preProcessor;

    public UpdateOperationStrategy(IRecordPreProcessor preProcessor)
    {
        _preProcessor = preProcessor;
    }

    public TransactionOperation Create(object entity, ITable table) =>
        (txId, affected) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            IEntityMetadata metadata = table.Metadata;
            metadata.Validate(entity);

            IValueBuffer record = metadata.Conversion.ToValueBuffer(entity);
            IValueBuffer processed = _preProcessor.Process(
                record,
                txId,
                out IEnumerable<ResolvedForeignKey> keys
            );

            foreach (ResolvedForeignKey key in keys)
            {
                ValidateForeignKey(key, txId);
            }

            table.Update(processed, txId);
            affected.Add(table);
        };

    private static void ValidateForeignKey(ResolvedForeignKey key, long txId)
    {
        ITable parent = key.Parent;
        if (!parent.Tracker.TryGetChange(key.PrimaryKey, txId, out _))
        {
            throw new RecordNotFoundException(Strings.RowNotFound(parent.Name, key.PrimaryKey));
        }
    }
}