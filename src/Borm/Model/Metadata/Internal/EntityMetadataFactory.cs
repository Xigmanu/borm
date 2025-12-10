using Borm.Model.Conversion;
using Borm.Reflection;

namespace Borm.Model.Metadata.Internal;

internal sealed class EntityMetadataFactory : MetadataFactory<EntityInfo, IEntityMetadata>
{
    private readonly MetadataFactory<IMappable, IColumnMetadata> _columnMetadataFactory;
    private readonly IEntityBufferConversionFactory _conversionFactory;

    public EntityMetadataFactory(
        IEntityBufferConversionFactory conversionFactory,
        MetadataFactory<IMappable, IColumnMetadata> columnMetadataFactory
    )
    {
        _conversionFactory = conversionFactory;
        _columnMetadataFactory = columnMetadataFactory;
    }

    public override IEntityMetadata Create(EntityInfo source)
    {
        string name = !string.IsNullOrWhiteSpace(source.Name)
            ? source.Name
            : CreateDefaultName(source.Type.Name);

        List<IColumnMetadata> columns = source
            .Properties.Select(_columnMetadataFactory.Create)
            .OrderBy(column => column.Index)
            .ToList();
        ColumnMetadataList columnCollection = new(columns);

        IEntityBufferConversion conversion = _conversionFactory.Create(
            source.Type,
            source.Constructors,
            columns
        );

        return new EntityMetadata(name, source.Type, columnCollection, conversion, source.Validate);
    }
}