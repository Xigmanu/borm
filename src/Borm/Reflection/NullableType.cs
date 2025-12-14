using System.Diagnostics;
using System.Reflection;

namespace Borm.Reflection;

public sealed class NullableType
{
    public NullableType(Type rawType, bool isNullable)
    {
        RawType = rawType;
        IsNullable = isNullable;
    }

    public bool IsNullable { get; }
    public Type RawType { get; }

    public Type UnderlyingType
    {
        get
        {
            if (!RawType.IsValueType || !IsNullable)
            {
                return RawType;
            }

            Type? underlying = Nullable.GetUnderlyingType(RawType);
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
            _ => throw new NotSupportedException($"Member {member} is not supported"),
        };

        static bool IsNullable(NullabilityInfo info)
        {
            return info.ReadState == NullabilityState.Nullable;
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is NullableType other
            && RawType == other.RawType
            && UnderlyingType == other.UnderlyingType
            && IsNullable == other.IsNullable;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(RawType, UnderlyingType, IsNullable);
    }
}
