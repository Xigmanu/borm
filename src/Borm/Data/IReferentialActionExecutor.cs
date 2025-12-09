using Borm.Data.Storage;

namespace Borm.Data;

internal interface IReferentialActionExecutor
{
    ISet<ITable> Run(ITable table, object parentPk, long txId);
}