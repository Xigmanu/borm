using System.Diagnostics;
using Borm.Reflection;

namespace Borm.Model.Metadata.Internal;

internal sealed class ColumnMetadataFactory : MetadataFactory<IMappable, IColumnMetadata>
{
    public override IColumnMetadata Create(IMappable property)
    {
        MappingInfo? mapping = property.Mapping;
        Debug.Assert(mapping != null, "Unexpected null property mapping");

        string columnName = mapping.ColumnName ?? CreateDefaultName(property.MemberName);
        Constraints constraints = GetConstraints(property);

        ColumnMetadata metadata = new(
            mapping.ColumnIndex,
            columnName,
            property.MemberName,
            property.DataType,
            constraints
        );

        if (mapping.Reference == null)
        {
            return metadata;
        }

        metadata.Reference = mapping.Reference;
        metadata.OnDelete = mapping.OnDelete;

        return metadata;
    }

    private static Constraints GetConstraints(IMappable property)
    {
        Constraints constraints = Constraints.None;
        MappingInfo mapping = property.Mapping!;
        if (mapping.IsPrimaryKey)
        {
            constraints |= Constraints.PrimaryKey;
        }
        else if (property.DataType.IsNullable)
        {
            constraints |= Constraints.AllowDbNull;
        }

        if (mapping.IsUnique)
        {
            constraints |= Constraints.Unique;
        }

        return constraints;
    }
}