using System.Reflection;
using Borm.Model.Validation;
using Borm.Model.Validation.Expressions;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Construction;

internal sealed class EntityFactory<TEntity>
    where TEntity : class
{
    private readonly IConfigurationValidator<IReadOnlyList<MappingMember>> _configurationValidator;
    private readonly Type _entityType;
    private readonly ColumnValidatorFactoryContext _factoryContext;
    private readonly PropertyInfo[] _properties;

    public EntityFactory(
        IConfigurationValidator<IReadOnlyList<MappingMember>> configurationValidator,
        ColumnValidatorFactoryContext factoryContext
    )
    {
        _configurationValidator = configurationValidator;
        _factoryContext = factoryContext;
        _entityType = typeof(TEntity);
        if (_entityType.IsAbstract)
        {
            throw new InvalidOperationException(
                Strings.EntityTypeCannotBeAbstract(_entityType.FullName ?? _entityType.Name)
            );
        }

        _properties = _entityType.GetProperties();
    }

    public EntityInfo Create()
    {
        EntityAttribute entityAttribute =
            _entityType.GetCustomAttribute<EntityAttribute>()
            ?? throw new MemberAccessException(
                Strings.EntityTypeNotDecorated(_entityType.FullName!, nameof(EntityAttribute))
            );

        List<MappingMember> properties = [];
        for (int i = 0; i < _properties.Length; i++)
        {
            MappingMember? property = ParseProperty(_properties[i]);
            if (property != null)
            {
                properties.Add(property);
            }
        }

        _configurationValidator.Validate(properties);

        IReadOnlyList<Constructor> constructors = ConstructorParser.ParseAll(_entityType);

        ObjectValidator? validator = CreateValidatorFunc(_entityType, properties);

        return new EntityInfo(
            entityAttribute.Name,
            _entityType,
            properties.AsReadOnly(),
            constructors,
            validator
        );
    }

    private static MappingMember? ParseProperty(PropertyInfo propertyInfo)
    {
        ColumnAttribute? attribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();
        if (attribute == null)
        {
            return null;
        }

        NullableType type = NullableType.WrapMemberType(propertyInfo);
        ValidWhenAttribute? validationAttribute =
            propertyInfo.GetCustomAttribute<ValidWhenAttribute>();

        MappingMember property = new(
            propertyInfo.Name,
            type,
            MappingInfo.FromAttribute(attribute),
            validationAttribute?.GetValidatorInfo<TEntity>(propertyInfo)
        );

        return property;
    }

    private static ObjectValidator ParseValidator(Type validatorType)
    {
        if (!validatorType.IsAssignableTo(typeof(IObjectValidator<TEntity>)))
        {
            throw new ArgumentException(
                Strings.ValidatorDoesNotImplementInterface(
                    validatorType.FullName ?? validatorType.Name,
                    nameof(IObjectValidator<TEntity>)
                ),
                nameof(validatorType)
            );
        }

        if (validatorType.GetConstructor(Type.EmptyTypes) == null)
        {
            throw new ArgumentException(
                Strings.ValidatorNoPublicDefaultCtor(validatorType.FullName ?? validatorType.Name),
                nameof(validatorType)
            );
        }

        IObjectValidator<TEntity> validator =
            (IObjectValidator<TEntity>)Activator.CreateInstance(validatorType)!;

        return ValidatorFunctionWrapper.Wrap(validator);
    }

    private ObjectValidator? CreateValidatorFunc(
        Type entityType,
        IReadOnlyList<MappingMember> properties
    )
    {
        ValidatorAttribute? validatorAttribute =
            entityType.GetCustomAttribute<ValidatorAttribute>();

        if (validatorAttribute != null)
        {
            return ParseValidator(validatorAttribute.ValidatorType);
        }

        return new ColumnValidatorFactory(_factoryContext, entityType, properties).Create();
    }
}