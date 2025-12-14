using Borm.Model.Validation;
using Borm.Model.Validation.Expressions;
using Borm.Reflection;

namespace Borm.Model.Construction;

public sealed class EntityBuilder<TEntity>
    where TEntity : class
{
    private readonly IConfigurationValidator<IReadOnlyList<IMappable>> _configValidator;
    private readonly Type _entityType;
    private readonly ColumnValidatorFactoryContext _factoryContext;
    private readonly List<IMappable> _properties = [];

    private ObjectValidator? _entityValidator;
    private string? _name;

    internal EntityBuilder(
        IConfigurationValidator<IReadOnlyList<IMappable>> configValidator,
        ColumnValidatorFactoryContext factoryContext
    )
    {
        _configValidator = configValidator;
        _factoryContext = factoryContext;
        _entityType = typeof(TEntity);
    }

    internal EntityInfo Build()
    {
        _configValidator.Validate(_properties);

        List<IConstructor> constructors = _entityType
            .GetConstructors()
            .Select(ConstructorParser.Parse)
            .ToList();

        ColumnValidatorFactory validatorFactory = new(
            _factoryContext,
            new ValidationExpressionBuilder(_factoryContext),
            _entityType,
            _properties
        );
        return new EntityInfo(
            _name,
            _entityType,
            _properties.AsReadOnly(),
            constructors,
            _entityValidator ?? validatorFactory.Create()
        );
    }

    public EntityBuilder<TEntity> Column(
        Func<ColumnBuilder<TEntity>, ColumnBuilder<TEntity>> columnBuilder
    )
    {
        ColumnBuilder<TEntity> builder = columnBuilder(
            new ColumnBuilder<TEntity>(new ColumnConfigurationValidator<TEntity>())
        );

        IMappable column = builder.Build();
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