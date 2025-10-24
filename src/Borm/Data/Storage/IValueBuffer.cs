using Borm.Model.Metadata;

namespace Borm.Data.Storage;

internal interface IValueBuffer : IEnumerable<KeyValuePair<IColumnMetadata, object>>
{
    object PrimaryKey { get; }
    object this[IColumnMetadata column] { get; set; }
    object this[string columnName] { get; }

    IValueBuffer Copy();
}
