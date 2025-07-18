using System.Reflection;
using Borm.Extensions;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Schema;

public sealed class EntityModel
{
    private readonly Assembly _modelSource;

    public EntityModel(Assembly modelSource)
    {
        ArgumentNullException.ThrowIfNull(modelSource);
        _modelSource = modelSource;
    }

    internal IEnumerable<ReflectedTypeInfo> GetReflectedInfo()
    {
        IEnumerable<Type> entityTypes = _modelSource.ExportedTypes.Where(type =>
            type.HasAttribute<EntityAttribute>()
        );
        List<ReflectedTypeInfo> reflectedInfos = new(entityTypes.Count());
        MetadataParser parser = new();
        foreach (Type entityType in entityTypes)
        {
            if (entityType.IsAbstract)
            {
                throw new ArgumentException(
                    Strings.EntityTypeCannotBeAbstract(entityType.FullName!)
                );
            }

            ReflectedTypeInfo reflectedInfo = parser.Parse(entityType);
            reflectedInfos.Add(reflectedInfo);
        }

        return reflectedInfos;
    }
}
