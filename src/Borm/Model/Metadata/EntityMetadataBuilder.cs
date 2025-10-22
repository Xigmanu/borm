using System.Data;
using System.Diagnostics;
using Borm.Data.Storage;
using Borm.Model.Metadata.Conversion;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Metadata;

internal static class EntityMetadataBuilder
{
    public static EntityMetadata Build(EntityTypeInfo entityType)
    {
        string name = !string.IsNullOrWhiteSpace(entityType.Name)
            ? entityType.Name
            : CreateDefaultName(entityType.Type.Name);

        IEnumerable<ColumnMetadata> columns = entityType
            .Properties.Select(CreateColumnInfo)
            .OrderBy(column => column.Index);
        ColumnMetadataList columnCollection = new(columns);

        IEntityBufferConversion conversion = CreateConversion(entityType, columns);

        return new EntityMetadata(name, entityType.Type, columnCollection, conversion);
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

    private static IEntityBufferConversion CreateConversion(
        EntityTypeInfo entityType,
        IEnumerable<ColumnMetadata> columns
    )
    {
        ConverterFactory<Func<object, IValueBuffer>> bufferConverter =
            new ValueBufferConverterFactory(entityType.Type, columns);

        Constructor? conversionCtor =
            ConstructorSelector.FindMappingCtor(
                entityType.Constructors,
                [.. columns.Select(col => col.Name)]
            )
            ?? throw new MissingMethodException(
                Strings.InvalidEntityTypeConstructor(entityType.Type.FullName!)
            );

        ConverterFactory<Func<IValueBuffer, object>> materializer = conversionCtor.IsDefault
            ? new PropertyConverterFactory(entityType.Type, columns)
            : new ConstructorConverterFactory(conversionCtor, columns);

        return new EntityBufferConversion(materializer.Create(), bufferConverter.Create());
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
