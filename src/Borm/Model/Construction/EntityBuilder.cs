using Borm.Reflection;

namespace Borm.Model.Construction;

public sealed class EntityBuilder<TEntity>
    where TEntity : class
{
    private readonly List<MappingMember> _columns = [];
    private string? _name;
    private IEntityValidator<TEntity>? _validator;

    public EntityInfo Build()
    {
        if (_columns.Count == 0)
        {
            throw new InvalidOperationException("Cannot create an entity with no columns");
        }

        IReadOnlyList<Constructor> constructors = ConstructorParser.ParseAll(typeof(TEntity));
        Action<object>? validatorAction =
            _validator != null ? (e) => _validator.Validate((TEntity)e) : null;

        return new EntityInfo(
            _name,
            typeof(TEntity),
            _columns.AsReadOnly(),
            constructors,
            validatorAction
        );
    }

    public EntityBuilder<TEntity> Column(
        Func<ColumnBuilder<TEntity>, ColumnBuilder<TEntity>> columnBuilder
    )
    {
        MappingMember column = columnBuilder(new ColumnBuilder<TEntity>()).Build();
        if (_columns.Any(c => c.MemberName == column.MemberName))
        {
            throw new ArgumentException(
                $"Column {column.MemberName} is already defined for entity of type {typeof(TEntity).FullName}"
            );
        }
        _columns.Add(column);
        return this;
    }

    public EntityBuilder<TEntity> Name(string entityName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        _name = entityName;
        return this;
    }

    public EntityBuilder<TEntity> Validator(IEntityValidator<TEntity> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        _validator = validator;
        return this;
    }
}
