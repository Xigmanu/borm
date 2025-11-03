using System.Diagnostics;
using System.Reflection;

namespace Borm.Reflection;

public sealed class NullableType
{
    private readonly Type _type;

    public NullableType(Type type, bool isNullable)
    {
        _type = type;
        IsNullable = isNullable;
    }

    public bool IsNullable { get; }
    public Type Type => _type;
    public Type UnderlyingType
    {
        get
        {
            if (!_type.IsValueType || !IsNullable)
            {
                return _type;
            }

            Type? underyling = Nullable.GetUnderlyingType(_type);
            Debug.Assert(underyling != null);
            return underyling;
        }
    }

    public static NullableType WrapMemberType(ICustomAttributeProvider member)
    {
        NullabilityInfoContext context = new();
        static bool isNullable(NullabilityInfo info) => info.ReadState == NullabilityState.Nullable;
        return member switch
        {
            PropertyInfo property => new NullableType(
                property.PropertyType,
                isNullable(context.Create(property))
            ),
            ParameterInfo parameter => new NullableType(
                parameter.ParameterType,
                isNullable(context.Create(parameter))
            ),
            _ => throw new NotSupportedException(),
        };
    }
}
