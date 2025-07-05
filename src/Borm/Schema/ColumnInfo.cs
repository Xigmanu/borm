using System.Reflection;

namespace Borm.Schema;

internal sealed class ColumnInfo
{
    private readonly Constraints _constraints;
    private readonly Type _dataType;
    private readonly int _index;
    private readonly string _name;
    private readonly Type? _referencedEntityType;
    private readonly MethodInfo _valueGetter;
    private readonly MethodInfo? _valueSetter;

    public ColumnInfo(
        int index,
        string name,
        Type dataType,
        MethodInfo valueGetter,
        MethodInfo? valueSetter,
        Constraints constraints,
        Type? referencedEntityType
    )
    {
        _index = index;
        _name = name;
        _referencedEntityType = referencedEntityType;
        _dataType = dataType;
        _valueGetter = valueGetter;
        _valueSetter = valueSetter;
        _constraints = constraints;
    }

    public Type DataType => _dataType;

    public int Index => _index;

    public string Name => _name;

    public Type? ReferencedEntityType => _referencedEntityType;

    internal Constraints Constraints => _constraints;

    public object? GetValue(object entityObj)
    {
        return _valueGetter.Invoke(entityObj, null);
    }

    public void SetValue(object entityObj, object? value)
    {
        _valueSetter?.Invoke(entityObj, [value]);
    }
}
