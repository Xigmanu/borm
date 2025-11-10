using System.Diagnostics;
using System.Reflection;

namespace Borm.Reflection;

public sealed class NullableType
{
    public NullableType(Type type, bool isNullable)
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
            if (!Type.IsValueType || !IsNullable)
            {
                return Type;
            }

            Type? underlying = Nullable.GetUnderlyingType(Type);
            Debug.Assert(underlying != null);
            return underlying;
        }
    }

    public static NullableType WrapMemberType(ICustomAttributeProvider member)
    {
        NullabilityInfoContext context = new();
        return member switch
        {
            PropertyInfo property => new NullableType(
                property.PropertyType,
                IsNullable(context.Create(property))
            ),
            ParameterInfo parameter => new NullableType(
                parameter.ParameterType,
                IsNullable(context.Create(parameter))
            ),
            _ => throw new NotSupportedException($"Member {member} is not supported")
        };

        static bool IsNullable(NullabilityInfo info)
        {
            return info.ReadState == NullabilityState.Nullable;
        }
    }
}