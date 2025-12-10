using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Model.Metadata.Internal;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Conversion.Internal;

internal sealed class EntityBufferConversionFactory : IEntityBufferConversionFactory
{
    public IEntityBufferConversion Create(
        Type entityType,
        IReadOnlyList<IConstructor> constructors,
        IReadOnlyList<IColumnMetadata> columns
    )
    {
        ConverterFactory<Func<object, IValueBuffer>> bufferConverter =
            new ValueBufferConverterFactory(entityType, columns);

        IConstructor conversionCtor =
            ConstructorSelector.FindMappingCtor(constructors, [.. columns.Select(col => col.Name)])
            ?? throw new MissingMethodException(
                Strings.InvalidEntityTypeConstructor(entityType.FullName ?? entityType.Name)
            );

        ConverterFactory<Func<IValueBuffer, object>> materializer = conversionCtor.IsDefault
            ? new PropertyConverterFactory(entityType, columns)
            : new ConstructorConverterFactory(conversionCtor, columns);

        return new EntityBufferConversion(materializer.Create(), bufferConverter.Create());
    }
}