using System.Linq.Expressions;
using System.Reflection;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Construction;

public sealed class ColumnBuilder<TEntity>
    where TEntity : class
{
    private readonly Type _entityType = typeof(TEntity);
    private string? _columnName;
    private NullableType? _dataType;
    private int _index;
    private bool _isPrimaryKey;
    private bool _isUnique;
    private string? _propName;
    private ReferentialAction _refAction;
    private Type? _reference;

    public MappingMember Build()
    {
        if (string.IsNullOrWhiteSpace(_propName))
        {
            throw new InvalidOperationException();
        }

        MappingInfo mappingInfo = new(
            _index,
            _columnName,
            _isPrimaryKey,
            _isUnique,
            _reference,
            _refAction
        );
        return new MappingMember(_propName!, _dataType!, mappingInfo);
    }

    public ColumnBuilder<TEntity> Index(int index)
    {
        _index = index < 0 ? throw new ArgumentException(Strings.InvalidColumnIndex()) : index;
        return this;
    }

    public ColumnBuilder<TEntity> Mapping<TProperty>(
        Expression<Func<TEntity, TProperty>> propProvider
    )
    {
        ResolvePropertyFromExpression(propProvider);
        return this;
    }

    public ColumnBuilder<TEntity> Mapping<TProperty>(
        Expression<Func<TEntity, TProperty>> propProvider,
        string columnName
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
        ResolvePropertyFromExpression(propProvider);
        _columnName = columnName;

        return this;
    }

    public ColumnBuilder<TEntity> OnDelete(ReferentialAction refAction)
    {
        _refAction = refAction;
        return this;
    }

    public ColumnBuilder<TEntity> PrimaryKey()
    {
        if (_reference != null)
        {
            throw new InvalidOperationException("Primary key cannot be a foreign key");
        }
        _isPrimaryKey = true;
        return this;
    }

    public ColumnBuilder<TEntity> References(Type parentType)
    {
        if (_isPrimaryKey)
        {
            throw new InvalidOperationException("Primary key cannot be a foreign key");
        }
        if (parentType == _entityType)
        {
            throw new ArgumentException("Circular Reference");
        }

        _reference = parentType;
        return this;
    }

    public ColumnBuilder<TEntity> Unique()
    {
        _isUnique = true;
        return this;
    }

    private void ResolvePropertyFromExpression<TProperty>(
        Expression<Func<TEntity, TProperty>> propProvider
    )
    {
        ArgumentNullException.ThrowIfNull(propProvider);
        MemberExpression member =
            propProvider.Body as MemberExpression
            ?? throw new ArgumentException(
                "Only member expressions: `e => e.Property` are allowed"
            );
        PropertyInfo property =
            member.Member as PropertyInfo
            ?? throw new MemberAccessException(
                $"No public property '{member.Member.Name}' is declared in a type '{_entityType.FullName}'"
            );

        _propName = property.Name;
        _dataType = NullableType.WrapMemberType(property);
    }
}
