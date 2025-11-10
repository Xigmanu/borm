using System.Linq.Expressions;
using System.Reflection;

namespace Borm.Reflection;

internal static class ConstructorParser
{
    public static IReadOnlyList<Constructor> ParseAll(Type entityType) =>
        entityType.GetConstructors().Select(ParseConstructorInfo).ToList().AsReadOnly();

    private static Constructor ParseConstructorInfo(ConstructorInfo ctor)
    {
        List<MappingMember> parsedParams = [];
        ParameterInfo[] parameters = ctor.GetParameters();
        parsedParams.AddRange(from param in parameters
            let type = NullableType.WrapMemberType(param)
            select new MappingMember(param.Name!, type, null));

        return new Constructor(
            parameters.Length == 0,
            parsedParams.AsReadOnly(),
            args => Expression.New(ctor, args)
        );
    }
}