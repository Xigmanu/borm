using System.Data;
using Borm.Reflection;

namespace Borm.Model.Metadata;

internal static class EntityMetadataBuilder
{
    public static EntityMetadata Build(ReflectedTypeInfo entityMetadata)
    {
        EntityAttribute entityAttribute = entityMetadata.Attribute;
        string name = entityAttribute.Name ?? CreateDefaultName(entityMetadata.Type.Name);

        IEnumerable<ColumnMetadata> columns = entityMetadata
            .Properties.Select(CreateColumnInfo)
            .OrderBy(column => column.Index);
        ColumnMetadataCollection columnCollection = new(columns);

        return new EntityMetadata(name, entityMetadata.Type, columnCollection);
    }

    private static ColumnMetadata CreateColumnInfo(Property property)
    {
        ColumnAttribute columnAttribute = property.Attribute;

        string? columnName = columnAttribute.Name ?? CreateDefaultName(property.Name);

        Constraints constraints = GetConstraints(property);

        ColumnMetadata columnMetadata = new(
            columnAttribute.Index,
            columnName,
            property.Name,
            property.Type,
            constraints
        );

        if (columnAttribute is ForeignKeyAttribute foreignKeyAttribute)
        {
            columnMetadata.Reference = foreignKeyAttribute.Reference;
            columnMetadata.OnUpdate = foreignKeyAttribute.OnUpdate;
            columnMetadata.OnDelete = foreignKeyAttribute.OnDelete;
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

    private static Constraints GetConstraints(Property property)
    {
        Constraints constraints = Constraints.None;
        ColumnAttribute attribute = property.Attribute;
        if (attribute is PrimaryKeyAttribute)
        {
            constraints |= Constraints.PrimaryKey;
        }
        else if (property.IsNullable)
        {
            constraints |= Constraints.AllowDbNull;
        }
        if (attribute.IsUnique)
        {
            constraints |= Constraints.Unique;
        }

        return constraints;
    }
}
