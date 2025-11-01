using Borm.Model;

namespace Borm.Reflection;

public sealed record MappingInfo(
    int ColumnIndex,
    string? ColumnName,
    bool IsPrimaryKey,
    bool IsUnique,
    Type? Reference,
    ReferentialAction OnDeleteAction
)
{
    internal static MappingInfo FromAttribute(ColumnAttribute attribute)
    {
        int idx = attribute.Index;
        string? name = attribute.Name;

        Type? reference = null;
        ReferentialAction action = ReferentialAction.NoAction;

        if (attribute is ForeignKeyAttribute fKAttribute)
        {
            action = fKAttribute.OnDelete;
            reference = fKAttribute.Reference;
        }

        return new MappingInfo(
            idx,
            name,
            attribute is PrimaryKeyAttribute,
            attribute.IsUnique,
            reference,
            action
        );
    }
}
