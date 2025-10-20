using System.Data;
using System.Diagnostics;
using Borm.Reflection;

namespace Borm.Model.Metadata;

internal static class EntityMetadataBuilder
{
    public static EntityMetadata Build(EntityType entityType)
    {
        string name = !string.IsNullOrWhiteSpace(entityType.Name)
            ? entityType.Name
            : CreateDefaultName(entityType.Type.Name);

        IOrderedEnumerable<ColumnMetadata> columns = entityType
            .Properties.Select(CreateColumnInfo)
            .OrderBy(column => column.Index);
        ColumnMetadataList columnCollection = new(columns);

        return new EntityMetadata(name, entityType.Type, columnCollection);
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
            property.TypeInfo.Type,
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
