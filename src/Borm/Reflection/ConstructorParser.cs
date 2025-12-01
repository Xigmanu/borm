using System.Linq.Expressions;
using System.Reflection;
using Borm.Reflection.Internal;

namespace Borm.Reflection;

internal static class ConstructorParser
{
    public static IConstructor Parse(ConstructorInfo ctor)
    {
        List<IMappable> parsedParams = [];
        ParameterInfo[] parameters = ctor.GetParameters();
        parsedParams.AddRange(
            from param in parameters
            let type = NullableType.WrapMemberType(param)
            select new Parameter(param.Name!, type)
        );

        return new Constructor(
            parameters.Length == 0,
            parsedParams.AsReadOnly(),
            args => Expression.New(ctor, args)
        );
    }
}