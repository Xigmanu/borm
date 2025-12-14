using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Model.Conversion;

internal interface IEntityBufferConversionFactory
{
    IEntityBufferConversion Create(
        Type entityType,
        IReadOnlyList<IConstructor> constructors,
        IReadOnlyList<IColumnMetadata> columns
    );
}