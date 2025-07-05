using System.Reflection;

namespace Borm.Extensions;

internal static class MemberInfoExtensions
{
    public static T GetAttributeOrThrow<T>(this MemberInfo memberInfo)
        where T : Attribute
    {
        return memberInfo.GetCustomAttribute<T>()
            ?? throw new MemberAccessException(
                $"Attribute {typeof(T).FullName} was not applied to the member {memberInfo.Name}"
            );
    }

    public static bool HasAttribute<T>(this MemberInfo memberInfo)
        where T : Attribute
    {
        return memberInfo.GetCustomAttribute<T>() != null;
    }
}
