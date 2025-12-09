using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data.Strategies.Internal;

internal class InsertOperationStrategy : IDataOperationStrategy
{
    private readonly IRecordPreProcessor _preProcessor;

    public InsertOperationStrategy(IRecordPreProcessor preProcessor)
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

            InsertRecursively(table, record, txId, affected);
        };

    private void InsertRecursively(
        ITable table,
        IValueBuffer record,
        long txId,
        ISet<ITable> affected
    )
    {
        IValueBuffer processed = _preProcessor.Process(
            record,
            txId,
            out IEnumerable<ResolvedForeignKey> keys
        );
        foreach (
            (
                ITable parent,
                object primaryKey,
                object rawValue,
                bool isComplexRecord,
                bool changeExists
            ) in keys
        )
        {
            if (changeExists)
            {
                continue;
            }

            IEntityMetadata metadata = parent.Metadata;
            if (!isComplexRecord)
            {
                throw new RecordNotFoundException(Strings.RowNotFound(parent.Name, primaryKey));
            }

            metadata.Validate(rawValue);
            IValueBuffer parentRecord = metadata.Conversion.ToValueBuffer(rawValue);
            InsertRecursively(parent, parentRecord, txId, affected);
        }

        table.Insert(processed, txId);
        affected.Add(table);
    }
}