using System.Diagnostics;
using System.Reflection;

namespace Borm.Extensions;

internal static class PropertyInfoExtensions
{
    public static Type UnwrapNullableType(this PropertyInfo propertyInfo, out bool isNullable)
    {
        NullabilityInfo nullabilityInfo = new NullabilityInfoContext().Create(propertyInfo);
        isNullable = nullabilityInfo.ReadState == NullabilityState.Nullable;
        Type propertyType = propertyInfo.PropertyType;
        if (!(isNullable && propertyType.IsValueType))
        {
            return propertyType;
        }
        Type? underlyingType = Nullable.GetUnderlyingType(propertyType);
        Debug.Assert(underlyingType != null);
        return underlyingType;
    }
}
