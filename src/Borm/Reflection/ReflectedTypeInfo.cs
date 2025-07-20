using Borm.Model;

namespace Borm.Reflection;

internal sealed class ReflectedTypeInfo
{
    public ReflectedTypeInfo(
        Type type,
        EntityAttribute attribute,
        IEnumerable<Property> properties
    )
    {
        Type = type;
        Attribute = attribute;
        Properties = properties;
    }

    public EntityAttribute Attribute { get; }
    public IEnumerable<Property> Properties { get; }
    public Type Type { get; }
}
