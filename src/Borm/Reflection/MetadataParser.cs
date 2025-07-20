using System.Reflection;
using Borm.Extensions;
using Borm.Model;

namespace Borm.Reflection;

internal sealed class MetadataParser
{
    private readonly NullabilityInfoContext _nullabilityCtx;

    public MetadataParser()
    {
        _nullabilityCtx = new();
    }

    public ReflectedTypeInfo Parse(Type entityType)
    {
        EntityAttribute entityAttribute =
            entityType.GetCustomAttribute<EntityAttribute>()
            ?? throw new MemberAccessException(
                $"EntityAttribute was not applied to the member {entityType.FullName}"
            );

        List<Property> properties = [];
        PropertyInfo[] typeProperties = entityType.GetProperties();
        for (int i = 0; i < typeProperties.Length; i++)
        {
            PropertyInfo propertyInfo = typeProperties[i];
            if (propertyInfo.HasAttribute<ColumnAttribute>())
            {
                properties.Add(ParsePropertyInfo(propertyInfo));
            }
        }

        return new ReflectedTypeInfo(entityType, entityAttribute, properties);
    }

    private Property ParsePropertyInfo(PropertyInfo propertyInfo)
    {
        ColumnAttribute columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>()!;
        bool isNullable =
            _nullabilityCtx.Create(propertyInfo).ReadState == NullabilityState.Nullable;
        return new Property(
            propertyInfo.Name,
            columnAttribute,
            isNullable,
            propertyInfo.PropertyType
        );
    }
}
