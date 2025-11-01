using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Borm.Reflection;

namespace Borm.Model.Construction;

public sealed class ColumnBuilder<T>
    where T : class
{
    private readonly Type _entityType = typeof(T);
    private readonly NullabilityHelper _nullabilityHelper = new();
    private string? _columnName;
    private NullableType? _dataType;
    private int _index;
    private bool _isPrimaryKey;
    private bool _isUnique;
    private string? _memberName;
    private ReferentialAction _refAction;
    private Type? _reference;

    public MappingMember Build()
    {
        ValidateConfiguration();
        Debug.Assert(!string.IsNullOrWhiteSpace(_memberName) && _dataType != null);

        MappingInfo mappingInfo = new(
            _index,
            _columnName,
            _isPrimaryKey,
            _isUnique,
            _reference,
            _refAction
        );
        return new MappingMember(_memberName, _dataType, mappingInfo);
    }

    public ColumnBuilder<T> Index(int index)
    {
        _index = index;
        return this;
    }

    public ColumnBuilder<T> Mapping([CallerMemberName] string? memberName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberName);
        ParseMemberFromName(memberName);
        return this;
    }

    public ColumnBuilder<T> Mapping(string columnName, [CallerMemberName] string? memberName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(memberName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        _columnName = columnName;
        ParseMemberFromName(memberName);

        return this;
    }

    public ColumnBuilder<T> OnDelete(ReferentialAction refAction)
    {
        _refAction = refAction;
        return this;
    }

    public ColumnBuilder<T> PrimaryKey()
    {
        _isPrimaryKey = true;
        return this;
    }

    public ColumnBuilder<T> References<TParent>()
    {
        Type parentType = typeof(TParent);
        if (parentType == _entityType)
        {
            throw new ArgumentException("Circular Reference");
        }

        _reference = parentType;
        return this;
    }

    public ColumnBuilder<T> Unique()
    {
        _isUnique = true;
        return this;
    }

    private void ParseMemberFromName(string memberName)
    {
        _memberName = memberName;

        PropertyInfo? prop =
            _entityType.GetProperties().FirstOrDefault(prop => prop.Name == memberName)
            ?? throw new ArgumentException(
                $"No public property '{memberName}' is declared in a type '{_entityType.FullName}'"
            );
        _dataType = _nullabilityHelper.WrapMemberType(prop);
    }

    private void ValidateConfiguration() // TODO
    {
        if (string.IsNullOrWhiteSpace(_memberName))
        {
            throw new InvalidOperationException();
        }
        if (_dataType == null)
        {
            throw new InvalidOperationException();
        }
    }
}
