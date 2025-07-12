using System.Reflection;
using Borm.Schema;

namespace Borm.Reflection;

internal sealed class ReflectedEntityInfo
{
    public ReflectedEntityInfo(
        Type type,
        EntityAttribute attribute,
        IEnumerable<EntityProperty> properties
    )
    {
        Type = type;
        Attribute = attribute;
        Properties = properties;
    }

    public EntityAttribute Attribute { get; }
    public IEnumerable<EntityProperty> Properties { get; }
    public Type Type { get; }
}
