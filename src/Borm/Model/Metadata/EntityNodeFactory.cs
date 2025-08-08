using System.Data;
using Borm.Reflection;

namespace Borm.Model.Metadata;

internal static class EntityNodeFactory
{
    public static EntityNode Create(ReflectedTypeInfo entityInfo)
    {
        EntityAttribute entityAttribute = entityInfo.Attribute;
        string name = entityAttribute.Name ?? CreateDefaultName(entityInfo.Type.Name);

        IEnumerable<ColumnInfo> columns = entityInfo
            .Properties.Select(CreateColumnInfo)
            .OrderBy(columnInfo => columnInfo.Index);
        ColumnInfoCollection columnCollection = new(columns);

        return new EntityNode(name, entityInfo.Type, columnCollection);
    }

    private static ColumnInfo CreateColumnInfo(Property property)
    {
        ColumnAttribute columnAttribute = property.Attribute;

        string? columnName = columnAttribute.Name ?? CreateDefaultName(property.Name);

        Constraints constraints = GetConstraints(property);
        Type? reference = FindReferencedEntityType(columnAttribute);

        return new ColumnInfo(
            columnAttribute.Index,
            columnName,
            property.Name,
            property.Type,
            constraints,
            reference
        );
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

    private static Type? FindReferencedEntityType(ColumnAttribute columnAttribute)
    {
        return columnAttribute is ForeignKeyAttribute foreignKeyAttribute
            ? foreignKeyAttribute.Reference
            : null;
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
        return constraints;
    }
}
