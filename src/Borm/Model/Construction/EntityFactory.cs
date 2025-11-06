using System.Reflection;
using Borm.Model.Validators;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Construction;

internal static class EntityFactory<TEntity>
    where TEntity : class
{
    public static EntityInfo Create(IValidator<IReadOnlyList<MappingMember>> validator)
    {
        return InternalCreate(validator, null);
    }

    public static EntityInfo Create(
        IValidator<IReadOnlyList<MappingMember>> validator,
        IValidator<TEntity> entityValidator
    )
    {
        Action<object>? validate =
            entityValidator != null ? (e) => entityValidator.Validate((TEntity)e) : null;

        return InternalCreate(validator, validate);
    }

    private static EntityInfo InternalCreate(
        IValidator<IReadOnlyList<MappingMember>> validator,
        Action<object>? validate
    )
    {
        Type entityType = typeof(TEntity);
        EntityAttribute entityAttribute =
            entityType.GetCustomAttribute<EntityAttribute>()
            ?? throw new MemberAccessException(
                Strings.EntityTypeNotDecorated(entityType.FullName!, nameof(EntityAttribute))
            );

        List<MappingMember> properties = ParseProperties(entityType);
        validator.Validate(properties);

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
