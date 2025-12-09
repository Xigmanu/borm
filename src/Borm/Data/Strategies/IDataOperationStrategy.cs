using Borm.Data.Storage;

namespace Borm.Data.Strategies;

internal interface IDataOperationStrategy
{
    TransactionOperation Create(object entity, ITable table);
}