using Borm.Model.Validators;
using Borm.Reflection;

namespace Borm.Model.Construction;

public sealed class EntityBuilder<TEntity>
    where TEntity : class
{
    private readonly List<MappingMember> _properties = [];
    private readonly IValidator<IReadOnlyList<MappingMember>> _validator;

    internal EntityBuilder(IValidator<IReadOnlyList<MappingMember>> validator)
    {
        _validator = validator;
    }

    private IValidator<TEntity>? _entityValidator;
    private string? _name;

    public EntityInfo Build()
    {
        _validator.Validate(_properties);

        IReadOnlyList<Constructor> constructors = ConstructorParser.ParseAll(typeof(TEntity));
        Action<object>? validatorAction =
            _entityValidator != null ? (e) => _entityValidator.Validate((TEntity)e) : null;

        return new EntityInfo(
            _name,
            typeof(TEntity),
            _properties.AsReadOnly(),
            constructors,
            validatorAction
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

    public EntityBuilder<TEntity> Validator(IValidator<TEntity> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        _entityValidator = validator;
        return this;
    }
}
