using System.Data;
using System.Diagnostics;
using System.Reflection;
using Borm.Extensions;

namespace Borm.Schema.Metadata;

internal sealed class EntityNodeFactory
{
    private readonly Type _entityType;

    public EntityNodeFactory(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        _entityType = entityType;
    }

    public EntityNode Create()
    {
        EntityAttribute entityAttribute = _entityType.GetAttributeOrThrow<EntityAttribute>();
        Debug.Assert(entityAttribute != null);
        string name = entityAttribute.Name ?? CreateDefaultName(_entityType);

        IEnumerable<ColumnInfo> columns = _entityType
            .GetProperties()
            .Where(property => property.HasAttribute<ColumnAttribute>())
            .Select(CreateColumnInfo)
            .OrderBy(columnInfo => columnInfo.Index);
        ColumnInfoCollection columnCollection = new(columns);

        ConstructorInfo ctor = new ConstructorResolver(
            columnCollection,
            _entityType
        ).GetAllColumnsConstructor(out bool isAutoConstructor);

        EntityBindingInfo bindingInfo = new(_entityType, columnCollection, ctor);
        EntityConversionBinding binding = isAutoConstructor
            ? EntityConversionBinding.CreatePropertyBased(bindingInfo)
            : EntityConversionBinding.CreateConstructorBased(bindingInfo);

        return new EntityNode(name, _entityType, columnCollection, binding);
    }

    private static ColumnInfo CreateColumnInfo(PropertyInfo propertyInfo)
    {
        ColumnAttribute columnAttribute = propertyInfo.GetAttributeOrThrow<ColumnAttribute>();

        string? columnName = columnAttribute.Name ?? CreateDefaultName(propertyInfo);

        Constraints constraints = GetConstraints(columnAttribute);
        if (IsNullable(propertyInfo))
        {
            constraints |= Constraints.AllowDbNull;
        }
        Type? referencedEntityType = FindReferencedEntityType(columnAttribute);

        return new ColumnInfo(
            columnAttribute.Index,
            columnName,
            propertyInfo.Name,
            propertyInfo.PropertyType,
            constraints,
            referencedEntityType
        );
    }

    private static string CreateDefaultName(MemberInfo memberInfo)
    {
        string typeName = memberInfo.Name;
        char first = typeName[0];
        if (char.IsUpper(first))
        {
            return typeName.Length == 1
                ? char.ToLower(first).ToString()
                : char.ToLower(first) + typeName[1..];
        }
        return typeName;
    }

    private static Type? FindReferencedEntityType(ColumnAttribute columnAttribute)
    {
        return columnAttribute is ForeignKeyAttribute foreignKeyAttribute
            ? foreignKeyAttribute.ReferencedEntityType
            : null;
    }

    private static Constraints GetConstraints(ColumnAttribute columnAttribute)
    {
        Constraints constraints = Constraints.None;
        if (columnAttribute is PrimaryKeyAttribute)
        {
            constraints |= Constraints.PrimaryKey;
        }
        return constraints;
    }

    private static bool IsNullable(PropertyInfo propertyInfo)
    {
        NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(propertyInfo);
        return nullabilityInfo.ReadState == NullabilityState.Nullable;
    }
}
