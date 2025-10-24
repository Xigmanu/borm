using System.Data;
using System.Diagnostics;
using Borm.Data.Storage;
using Borm.Model.Metadata.Conversion;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Metadata;

internal static class EntityMetadataBuilder
{
    public static IEntityMetadata Build(EntityTypeInfo typeInfo)
    {
        string name = !string.IsNullOrWhiteSpace(typeInfo.Name)
            ? typeInfo.Name
            : CreateDefaultName(typeInfo.Type.Name);

        IEnumerable<IColumnMetadata> columns = typeInfo
            .Properties.Select(CreateColumnInfo)
            .OrderBy(column => column.Index);
        ColumnMetadataList columnCollection = new(columns);

        IEntityBufferConversion conversion = EntityBufferConversionFactory.Create(
            typeInfo,
            columns
        );

        return new EntityMetadata(
            name,
            typeInfo.Type,
            columnCollection,
            conversion,
            typeInfo.Validate
        );
    }

    private static ColumnMetadata CreateColumnInfo(MappingMember property)
    {
        MappingInfo? mapping = property.Mapping;
        Debug.Assert(mapping != null);
        string? columnName = mapping.ColumnName ?? CreateDefaultName(property.MemberName);

        Constraints constraints = GetConstraints(property);

        ColumnMetadata columnMetadata = new(
            mapping.ColumnIndex,
            columnName,
            property.MemberName,
            property.TypeInfo,
            constraints
        );

        if (mapping.IsForeignKey)
        {
            columnMetadata.Reference = mapping.Reference;
            columnMetadata.OnDelete = mapping.OnDeleteAction;
        }

        return columnMetadata;
    }

    private static string CreateDefaultName(string memberName)
    {
        char first = memberName[0];
        if (char.IsUpper(first))
        {
            return memberName.Length == 1
                ? char.ToLower(first).ToString()
                : char.ToLower(first) + memberName[1..];
        }
        return memberName;
    }

    private static Constraints GetConstraints(MappingMember property)
    {
        Constraints constraints = Constraints.None;
        MappingInfo mapping = property.Mapping!;
        if (mapping.IsPrimaryKey)
        {
            constraints |= Constraints.PrimaryKey;
        }
        else if (property.TypeInfo.IsNullable)
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
