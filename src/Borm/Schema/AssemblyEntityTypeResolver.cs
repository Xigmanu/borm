using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Borm.Extensions;

namespace Borm.Schema;

internal static class AssemblyEntityTypeResolver
{
    public static IEnumerable<Type> GetTypes(Assembly assembly)
    {
        IEnumerable<Type> entityTypes = assembly
            .GetExportedTypes()
            .Where(type => type.HasAttribute<TableAttribute>());
        if (!IsTypeEnumerableValid(entityTypes, out ArgumentException? exception))
        {
            throw exception;
        }
        Debug.WriteLine($"Resolved {entityTypes.Count()} entity types");
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
            if (type.IsValueType)
            {
                exception = new ArgumentException(
                    $"Entity types can only be reference types. Type: {type.FullName}"
                );
                return false;
            }
        }
        return true;
    }
}
