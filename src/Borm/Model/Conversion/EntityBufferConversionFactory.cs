using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Conversion;

internal static class EntityBufferConversionFactory
{
    public static IEntityBufferConversion Create(
        Type entityType,
        IReadOnlyList<Constructor> constructors,
        IEnumerable<IColumnMetadata> columns
    )
    {
        ConverterFactory<Func<object, IValueBuffer>> bufferConverter =
            new ValueBufferConverterFactory(entityType, columns);

        Constructor? conversionCtor =
            ConstructorSelector.FindMappingCtor(constructors, [.. columns.Select(col => col.Name)])
            ?? throw new MissingMethodException(
                Strings.InvalidEntityTypeConstructor(entityType.FullName!)
            );

        ConverterFactory<Func<IValueBuffer, object>> materializer = conversionCtor.IsDefault
            ? new PropertyConverterFactory(entityType, columns)
            : new ConstructorConverterFactory(conversionCtor, columns);

        return new EntityBufferConversion(materializer.Create(), bufferConverter.Create());
    }
}
