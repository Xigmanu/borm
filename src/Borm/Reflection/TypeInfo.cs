using System.Diagnostics;

namespace Borm.Reflection;

internal sealed class TypeInfo
{
    public TypeInfo(Type type, bool isNullable)
    {
        Type = type;
        IsNullable = isNullable;
    }

    public bool IsNullable { get; }
    public Type Type { get; }
    public Type UnderlyingType
    {
        get
        {
            if (!Type.IsValueType)
            {
                return Type;
            }

            Type? underyling = Nullable.GetUnderlyingType(Type);
            Debug.Assert(underyling != null);
            return underyling;
        }
    }
}
