using Borm.Model.Metadata.Conversion;

namespace Borm.Model.Metadata;

internal interface IEntityMetadata
{
    IReadOnlyList<IColumnMetadata> Columns { get; }
    IEntityBufferConversion Conversion { get; }
    string Name { get; }
    IColumnMetadata PrimaryKey { get; }
    Type Type { get; }
    void Validate(object entity);
}
