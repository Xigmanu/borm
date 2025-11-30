using Borm.Model.Validation;
using Borm.Model.Validation.Expressions;
using Borm.Reflection;

namespace Borm.Model.Construction;

public sealed class EntityBuilder<TEntity>
    where TEntity : class
{
    private readonly IConfigurationValidator<IReadOnlyList<MappingMember>> _configValidator;
    private readonly ColumnValidatorFactoryContext _factoryContext;
    private readonly List<MappingMember> _properties = [];

    private ObjectValidator? _entityValidator;
    private string? _name;

    internal EntityBuilder(
        IConfigurationValidator<IReadOnlyList<MappingMember>> configValidator,
        ColumnValidatorFactoryContext factoryContext
    )
    {
        _configValidator = configValidator;
        _factoryContext = factoryContext;
    }

    internal EntityInfo Build()
    {
        _configValidator.Validate(_properties);

        IReadOnlyList<Constructor> constructors = ConstructorParser.ParseAll(typeof(TEntity));

        return new EntityInfo(
            _name,
            typeof(TEntity),
            _properties.AsReadOnly(),
            constructors,
            _entityValidator
            ?? new ColumnValidatorFactory(
                _factoryContext,
                typeof(TEntity),
                _properties
            ).Create()
        );
    }

    public EntityBuilder<TEntity> Column(
        Func<ColumnBuilder<TEntity>, ColumnBuilder<TEntity>> columnBuilder
    )
    {
        ColumnBuilder<TEntity> builder = columnBuilder(
            new ColumnBuilder<TEntity>(new ColumnConfigurationValidator<TEntity>())
        );

        MappingMember column = builder.Build();
        if (_properties.Any(c => c.MemberName == column.MemberName))
        {
            throw new ArgumentException(
                $"Column {column.MemberName} is already defined for entity of type {typeof(TEntity).FullName}"
            );
        }

        _properties.Add(column);
        return this;
    }

    public EntityBuilder<TEntity> Name(string entityName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        _name = entityName;
        return this;
    }

    public EntityBuilder<TEntity> Validator(IObjectValidator<TEntity> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        _entityValidator = ValidatorFunctionWrapper.Wrap(validator);
        return this;
    }

    public EntityBuilder<TEntity> Validator(Func<TEntity, ValidationResult> validatorFunc)
    {
        ArgumentNullException.ThrowIfNull(validatorFunc);
        _entityValidator = ValidatorFunctionWrapper.Wrap(validatorFunc);
        return this;
    }
}