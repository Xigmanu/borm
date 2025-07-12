using Borm.Extensions;

namespace Borm.Schema;

internal static class EntityTypeResolver
{
    public static IEnumerable<Type> GetTypes(IEnumerable<Type> types)
    {
        List<Type> entityTypes = [.. types.Where(type => type.HasAttribute<EntityAttribute>())];
        ValidateEntityTypes(entityTypes);

        return entityTypes;
    }

    private static void ValidateEntityTypes(List<Type> entityTypes)
    {
        Type? abstractEntityType = entityTypes.FirstOrDefault(type => type.IsAbstract);
        if (abstractEntityType != null)
        {
            throw new ArgumentException(
                $"Entity classes cannot be abstract. Type: {abstractEntityType.FullName}"
            );
        }
    }
}
