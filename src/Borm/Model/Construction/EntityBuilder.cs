using System.Linq.Expressions;
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
        Expression<Func<ColumnBuilder<TEntity>, TEntity, ColumnBuilder<TEntity>>> columnBuilder
    )
    {
        MappingMember column = ColumnBuilderExpressionInterpreter<TEntity>
            .Interpret(columnBuilder)
            .Build();
        if (_columns.Contains(column))
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
