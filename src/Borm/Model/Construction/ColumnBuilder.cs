using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Borm.Model.Validation;
using Borm.Properties;
using Borm.Reflection;
using Borm.Reflection.Internal;

namespace Borm.Model.Construction;

public sealed class ColumnBuilder<TEntity>
    where TEntity : class
{
    private readonly IConfigurationValidator<Configuration> _configurationValidator;
    private readonly Type _entityType = typeof(TEntity);
    private string? _columnName;
    private int _index;
    private bool _isPrimaryKey;
    private bool _isUnique;
    private PropertyInfo? _property;
    private ReferentialAction _refAction;
    private Type? _reference;
    private LambdaExpression? _validationExpression;

    internal ColumnBuilder(IConfigurationValidator<Configuration> configurationValidator)
    {
        _configurationValidator = configurationValidator;
    }

    internal IMappable Build()
    {
        NullableType? type =
            _property?.PropertyType != null ? NullableType.WrapMemberType(_property) : null;
        string? propertyName = _property?.Name;
        _configurationValidator.Validate(
            new Configuration(propertyName, type, _isPrimaryKey, _reference)
        );

        MappingInfo mappingInfo = new(
            _index,
            _columnName,
            _isPrimaryKey,
            _isUnique,
            _reference,
            _refAction
        );
        return new Property(propertyName!, type!, mappingInfo, BuildValidationInfo());
    }

    private ValidationInfo? BuildValidationInfo()
    {
        if (_validationExpression == null)
        {
            return null;
        }

        Debug.Assert(_property != null);

        ParameterExpression lambdaParam = _validationExpression.Parameters[0];
        MemberExpression propertyAccessExpression = Expression.Property(lambdaParam, _property);

        return new ValidationInfo(_validationExpression, propertyAccessExpression);
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
        _isPrimaryKey = true;
        return this;
    }

    public ColumnBuilder<TEntity> References(Type parentType)
    {
        if (parentType == _entityType)
        {
            throw new ArgumentException(
                Strings.EntityDependencyCircularReference(_entityType.FullName ?? _entityType.Name)
            );
        }

        _reference = parentType;
        return this;
    }

    public ColumnBuilder<TEntity> Unique()
    {
        _isUnique = true;
        return this;
    }

    public ColumnBuilder<TEntity> ValidWhen(Expression<Func<TEntity, bool>> validationExpression)
    {
        ArgumentNullException.ThrowIfNull(validationExpression);
        _validationExpression = validationExpression;
        return this;
    }

    private void ResolvePropertyFromExpression<TProperty>(
        Expression<Func<TEntity, TProperty>> propProvider
    )
    {
        ArgumentNullException.ThrowIfNull(propProvider);
        MemberExpression member =
            propProvider.Body as MemberExpression
            ?? throw new ArgumentException(Strings.InvalidMemberExpression());
        PropertyInfo property =
            member.Member as PropertyInfo
            ?? throw new MemberAccessException(
                Strings.NoPublicPropertyDeclared(
                    member.Member.Name,
                    _entityType.FullName ?? _entityType.Name
                )
            );

        _property = property;
    }

    internal sealed record Configuration(
        string? PropertyName,
        NullableType? DataType,
        bool IsPrimaryKey,
        Type? Reference
    );
}