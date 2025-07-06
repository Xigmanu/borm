using System.Diagnostics.CodeAnalysis;
using Borm.Extensions;

namespace Borm.Schema;

internal static class EntityTypeResolver
{
    public static IEnumerable<Type> GetTypes(IEnumerable<Type> assemblyTypes)
    {
        IEnumerable<Type> entityTypes = assemblyTypes.Where(type =>
            type.HasAttribute<TableAttribute>()
        );
        if (!IsTypeEnumerableValid(entityTypes, out ArgumentException? exception))
        {
            throw exception;
        }

        return entityTypes;
    }

    private static bool IsTypeEnumerableValid(
        IEnumerable<Type> types,
        [NotNullWhen(false)] out ArgumentException? exception
    )
    {
        exception = null;
        foreach (Type type in types)
        {
            if (type.IsAbstract)
            {
                exception = new ArgumentException(
                    $"Entity classes cannot be abstract. Type: {type.FullName}"
                );
                return false;
            }
        }
        return true;
    }
}
