using System.Data;

namespace Borm.Data;

internal sealed class VersionedDataRow : DataRow
{
    internal VersionedDataRow(DataRowBuilder builder)
        : base(builder) { }

    public long InsertTx { get; set; }
}
