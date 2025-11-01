using System.Reflection;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Construction;

public static class EntityFactory
{
    public static EntityInfo FromType(Type entityType)
    {
        return InternalCreate(entityType, null);
    }

    public static EntityInfo FromType<T>(Type entityType, IEntityValidator<T> validator)
        where T : class
    {
        if (typeof(T) != entityType)
        {
            throw new ArgumentException($"Invalid validator for entity type {entityType.FullName}");
        }

        Action<object>? validate = validator != null ? (e) => validator.Validate((T)e) : null;

        return InternalCreate(entityType, validate);
    }

    private static EntityInfo InternalCreate(Type entityType, Action<object>? validate)
    {
        EntityAttribute entityAttribute =
            entityType.GetCustomAttribute<EntityAttribute>()
            ?? throw new MemberAccessException(
                Strings.EntityTypeNotDecorated(entityType.FullName!, nameof(EntityAttribute))
            );

        List<MappingMember> properties = ParseProperties(entityType);

        IReadOnlyList<Constructor> constructors = ConstructorParser.ParseAll(entityType);

        return new EntityInfo(
            entityAttribute.Name,
            entityType,
            properties.AsReadOnly(),
            constructors,
            validate
        );
    }

    private static List<MappingMember> ParseProperties(Type entityType)
    {
        NullabilityHelper typeHelper = new();
        List<MappingMember> properties = [];
        PropertyInfo[] typeProps = entityType.GetProperties();
        for (int i = 0; i < typeProps.Length; i++)
        {
            PropertyInfo current = typeProps[i];
            ColumnAttribute? attribute = current.GetCustomAttribute<ColumnAttribute>();
            if (attribute == null)
            {
                continue;
            }

            NullableType type = typeHelper.WrapMemberType(current);
            MappingMember property = new(current.Name, type, MappingInfo.FromAttribute(attribute));
            properties.Add(property);
        }

        return properties;
    }
}
