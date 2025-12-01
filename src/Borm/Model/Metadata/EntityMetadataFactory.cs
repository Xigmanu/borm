using System.Diagnostics;
using Borm.Model.Conversion;
using Borm.Reflection;

namespace Borm.Model.Metadata;

internal static class EntityMetadataFactory
{
    public static IEntityMetadata Create(
        EntityInfo typeInfo,
        Func<
            Type,
            IReadOnlyList<IConstructor>,
            IReadOnlyList<IColumnMetadata>,
            IEntityBufferConversion
        > conversionSupplierFunc
    )
    {
        string name = !string.IsNullOrWhiteSpace(typeInfo.Name)
            ? typeInfo.Name
            : CreateDefaultName(typeInfo.Type.Name);

        List<ColumnMetadata> columns = typeInfo
            .Properties.Select(CreateColumnInfo)
            .OrderBy(column => column.Index)
            .ToList();
        ColumnMetadataList columnCollection = new(columns);

        IEntityBufferConversion conversion = conversionSupplierFunc(
            typeInfo.Type,
            typeInfo.Constructors,
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

    private static ColumnMetadata CreateColumnInfo(IMappable property)
    {
        MappingInfo? mapping = property.Mapping;
        Debug.Assert(mapping != null);
        string columnName = mapping.ColumnName ?? CreateDefaultName(property.MemberName);

        Constraints constraints = GetConstraints(property);

        ColumnMetadata columnMetadata = new(
            mapping.ColumnIndex,
            columnName,
            property.MemberName,
            property.DataType,
            constraints
        );

        if (mapping.Reference == null)
        {
            return columnMetadata;
        }

        columnMetadata.Reference = mapping.Reference;
        columnMetadata.OnDelete = mapping.OnDelete;

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
