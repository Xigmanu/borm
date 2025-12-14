using Borm.Data.Storage;

namespace Borm.Data;

internal delegate void TransactionOperation(long txId, HashSet<ITable> affected);
