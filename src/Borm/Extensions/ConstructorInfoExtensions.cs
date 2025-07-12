using System.Reflection;

namespace Borm.Extensions;

internal static class ConstructorInfoExtensions
{
    public static bool IsNoArgs(this ConstructorInfo constructorInfo)
    {
        Type declaringType = constructorInfo.DeclaringType!;
        return constructorInfo == declaringType.GetConstructor(Type.EmptyTypes);
    }
}
