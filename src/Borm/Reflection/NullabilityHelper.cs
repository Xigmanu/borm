using System.Reflection;

namespace Borm.Reflection;

internal sealed class NullabilityHelper
{
    private readonly NullabilityInfoContext _context = new();

    public NullableType WrapMemberType(ICustomAttributeProvider member)
    {
        static bool isNull(NullabilityInfo info) => info.ReadState == NullabilityState.Nullable;
        return member switch
        {
            PropertyInfo prop => new NullableType(
                prop.PropertyType,
                isNull(_context.Create(prop))
            ),
            ParameterInfo param => new NullableType(
                param.ParameterType,
                isNull(_context.Create(param))
            ),
            _ => throw new NotSupportedException(),
        };
    }
}
