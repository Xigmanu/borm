using Borm.Data.Storage;

namespace Borm.Data;

internal interface IRecordPreProcessor
{
    IValueBuffer Process(
        IValueBuffer record,
        long txId,
        out IEnumerable<ResolvedForeignKey> keys
    );
}