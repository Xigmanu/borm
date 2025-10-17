using Borm.Model;

namespace Borm.Reflection;

internal sealed record MappingInfo(
    int ColumnIndex,
    string? ColumnName,
    bool IsForeignKey,
    bool IsPrimaryKey,
    bool IsUnique,
    Type? Reference,
    ReferentialAction OnDeleteAction
)
{
    public static MappingInfo FromAttribute(ColumnAttribute attribute)
    {
        int idx = attribute.Index;
        string? name = attribute.Name;

        Type? reference = null;
        bool isFK = false;
        ReferentialAction action = ReferentialAction.NoAction;

        if (attribute is ForeignKeyAttribute fKAttribute)
        {
            isFK = true;
            action = fKAttribute.OnDelete;
            reference = fKAttribute.Reference;
        }

        return new MappingInfo(
            idx,
            name,
            isFK,
            attribute is PrimaryKeyAttribute,
            attribute.IsUnique,
            reference,
            action
        );
    }
}
