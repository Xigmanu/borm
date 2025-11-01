using System.Linq.Expressions;
using System.Reflection;

namespace Borm.Reflection;

internal static class ConstructorParser
{
    public static IReadOnlyList<Constructor> ParseAll(Type entityType) =>
        entityType.GetConstructors().Select(ParseConstructorInfo).ToList().AsReadOnly();

    private static Constructor ParseConstructorInfo(ConstructorInfo ctor)
    {
        NullabilityHelper nullabilityHelper = new();
        List<MappingMember> parsedParams = [];
        ParameterInfo[] parameters = ctor.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo param = parameters[i];
            NullableType type = nullabilityHelper.WrapMemberType(param);
            MappingMember parsedParam = new(param.Name!, type, Mapping: null);
            parsedParams.Add(parsedParam);
        }

        return new Constructor(
            parameters.Length == 0,
            parsedParams.AsReadOnly(),
            (args) => Expression.New(ctor, args)
        );
    }
}
