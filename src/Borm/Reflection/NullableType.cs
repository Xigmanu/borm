using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Borm.Reflection;

internal sealed class NullableType
{
    private readonly Type _type;

    public NullableType(Type type, bool isNullable)
    {
        _type = type;
        IsNullable = isNullable;
    }

    public string FullName => UnderlyingType.FullName!;
    public bool IsNullable { get; }
    public Type Type => _type;
    public Type UnderlyingType
    {
        get
        {
            if (!_type.IsValueType)
            {
                return _type;
            }

            if (IsNullable)
            {
                Type? underyling = Nullable.GetUnderlyingType(_type);
                Debug.Assert(underyling != null);
                return underyling;
            }
            return _type;
        }
    }
}
