using Borm.Data.Storage;

namespace Borm.Model.Metadata.Conversion;

internal interface IEntityBufferConversion
{
    IValueBuffer ToValueBuffer(object entity);
    object MaterializeEntity(IValueBuffer buffer);
}
