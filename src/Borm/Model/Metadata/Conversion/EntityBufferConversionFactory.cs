using Borm.Data.Storage;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Metadata.Conversion;

internal static class EntityBufferConversionFactory
{
    public static IEntityBufferConversion Create(
        EntityTypeInfo entityType,
        IEnumerable<IColumnMetadata> columns
    )
    {
        ConverterFactory<Func<object, IValueBuffer>> bufferConverter =
            new ValueBufferConverterFactory(entityType.Type, columns);

        Constructor? conversionCtor =
            ConstructorSelector.FindMappingCtor(
                entityType.Constructors,
                [.. columns.Select(col => col.Name)]
            )
            ?? throw new MissingMethodException(
                Strings.InvalidEntityTypeConstructor(entityType.Type.FullName!)
            );

        ConverterFactory<Func<IValueBuffer, object>> materializer = conversionCtor.IsDefault
            ? new PropertyConverterFactory(entityType.Type, columns)
            : new ConstructorConverterFactory(conversionCtor, columns);

        return new EntityBufferConversion(materializer.Create(), bufferConverter.Create());
    }
}
