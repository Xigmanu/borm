using Borm.Properties;
using Borm.Reflection;
using System.Reflection;
using Borm.Model.Validation;

namespace Borm.Model.Construction;

internal static class EntityFactory<TEntity>
    where TEntity : class
{
    public static EntityInfo Create(IConfigurationValidator<IReadOnlyList<MappingMember>> configurationValidator)
    {
        return InternalCreate(configurationValidator, null);
    }

    public static EntityInfo Create(
        IConfigurationValidator<IReadOnlyList<MappingMember>> configurationValidator,
        Func<object, ValidationResult> entityValidator
    )
    {
        return InternalCreate(configurationValidator, entityValidator);
    }

    private static EntityInfo InternalCreate(
        IConfigurationValidator<IReadOnlyList<MappingMember>> configurationValidator,
        Func<object, ValidationResult>? validate
    )
    {
        Type entityType = typeof(TEntity);
        if (entityType.IsAbstract)
        {
            throw new InvalidOperationException(
                Strings.EntityTypeCannotBeAbstract(entityType.FullName ?? entityType.Name)
            );
        }

        EntityAttribute entityAttribute =
            entityType.GetCustomAttribute<EntityAttribute>()
            ?? throw new MemberAccessException(
                Strings.EntityTypeNotDecorated(entityType.FullName!, nameof(EntityAttribute))
            );

        List<MappingMember> properties = ParseProperties(entityType);
        configurationValidator.Validate(properties);

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

            NullableType type = NullableType.WrapMemberType(current);
            MappingMember property = new(current.Name, type, MappingInfo.FromAttribute(attribute));

            properties.Add(property);
        }

        return properties;
    }
}
