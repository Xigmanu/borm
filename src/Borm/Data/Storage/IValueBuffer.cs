using Borm.Model.Metadata;

namespace Borm.Data.Storage;

internal interface IValueBuffer : IEnumerable<KeyValuePair<ColumnMetadata, object>>
{
    object PrimaryKey { get; }
    object this[ColumnMetadata column] { get; set; }
    object this[string columnName] { get; }

    IValueBuffer Copy();
}
