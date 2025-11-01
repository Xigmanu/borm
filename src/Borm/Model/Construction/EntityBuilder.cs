using System.Linq.Expressions;
using Borm.Reflection;

namespace Borm.Model.Construction;

public sealed class EntityBuilder<T>
    where T : class
{
    private readonly List<MappingMember> _columns = [];
    private string? _name;
    private IEntityValidator<T>? _validator;

    public EntityInfo Build()
    {
        IReadOnlyList<Constructor> constructors = ConstructorParser.ParseAll(typeof(T));
        Action<object>? validatorAction =
            _validator != null ? (e) => _validator.Validate((T)e) : null;
        return new EntityInfo(
            _name,
            typeof(T),
            _columns.AsReadOnly(),
            constructors,
            validatorAction
        );
    }

    public EntityBuilder<T> Column(
        Expression<Func<ColumnBuilder<T>, T, ColumnBuilder<T>>> columnBuilder
    )
    {
        MappingMember column = ColumnBuilderExecutor.Execute(columnBuilder).Build();
        if (_columns.Contains(column))
        {
            throw new ArgumentException(
                $"Column {column.MemberName} is already defined for entity of type {typeof(T).FullName}"
            );
        }
        _columns.Add(column);
        return this;
    }

    public EntityBuilder<T> Name(string entityName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);
        _name = entityName;
        return this;
    }

    public EntityBuilder<T> Validator(IEntityValidator<T> validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        _validator = validator;
        return this;
    }
}
