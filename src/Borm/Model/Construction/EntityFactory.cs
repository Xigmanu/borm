using System.Reflection;
using Borm.Model.Validation;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Construction;

internal static class EntityFactory<TEntity>
    where TEntity : class
{
    public static EntityInfo Create(
        IConfigurationValidator<IReadOnlyList<MappingMember>> configurationValidator
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

        ValidatorAttribute? validatorAttribute = entityType.GetCustomAttribute<
            ValidatorAttribute
        >();

        return new EntityInfo(
            entityAttribute.Name,
            entityType,
            properties.AsReadOnly(),
            constructors,
            validatorAttribute != null
                ? ParseValidator(validatorAttribute.ValidatorType)
                : null
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
            MappingMember property = new(current.Name, type, MappingInfo.FromAttribute(attribute), null);

            properties.Add(property);
        }

        return properties;
    }

    private static Func<object, ValidationResult> ParseValidator(Type validatorType)
    {
        Type iFaceType = typeof(IObjectValidator<TEntity>);
        if (!validatorType.IsAssignableTo(iFaceType))
        {
            throw new ArgumentException(
                $"Validator type '{validatorType.FullName}' must implement '{iFaceType.FullName}'.",
                nameof(validatorType));
        }

        if (validatorType.GetConstructor(Type.EmptyTypes) == null)
        {
            throw new ArgumentException($"Validator '{validatorType.FullName}' must have a parameterless constructor.",
                nameof(validatorType));
        }

        IObjectValidator<TEntity> validator = (IObjectValidator<TEntity>)Activator.CreateInstance(validatorType)!;

        return ValidatorFunctionWrapper.Wrap(validator);
    }
}