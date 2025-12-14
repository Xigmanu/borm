using Borm.Model.Conversion;

namespace Borm.Model.Metadata;

internal interface IEntityMetadata : IMetadata
{
    IReadOnlyList<IColumnMetadata> Columns { get; }
    IEntityBufferConversion Conversion { get; }
    IColumnMetadata PrimaryKey { get; }
    Type Type { get; }

    IColumnMetadata GetColumn(string memberName);
    void Validate(object entity);
}