using System.Reflection;
using Borm.Extensions;
using Borm.Schema;

namespace Borm.Reflection;

internal sealed class EntityMetadataParser
{
    private readonly NullabilityInfoContext _nullabilityCtx;

    public EntityMetadataParser()
    {
        _nullabilityCtx = new();
    }

    public ReflectedEntityInfo Parse(Type entityType)
    {
        EntityAttribute entityAttribute =
            entityType.GetCustomAttribute<EntityAttribute>()
            ?? throw new MemberAccessException(
                $"EntityAttribute was not applied to the member {entityType.FullName}"
            );

        List<EntityProperty> properties = [];
        PropertyInfo[] typeProperties = entityType.GetProperties();
        for (int i = 0; i < typeProperties.Length; i++)
        {
            PropertyInfo propertyInfo = typeProperties[i];
            if (propertyInfo.HasAttribute<ColumnAttribute>())
            {
                properties.Add(ParsePropertyInfo(propertyInfo));
            }
        }

        return new ReflectedEntityInfo(entityType, entityAttribute, properties);
    }

    private EntityProperty ParsePropertyInfo(PropertyInfo propertyInfo)
    {
        ColumnAttribute columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>()!;
        bool isNullable =
            _nullabilityCtx.Create(propertyInfo).ReadState == NullabilityState.Nullable;
        return new EntityProperty(
            propertyInfo.Name,
            columnAttribute,
            isNullable,
            propertyInfo.PropertyType
        );
    }
}
