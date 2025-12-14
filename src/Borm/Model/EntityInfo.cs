using Borm.Model.Validation;
using Borm.Reflection;

namespace Borm.Model;

public sealed class EntityInfo
{
    internal EntityInfo(string? name,
        Type type,
        IReadOnlyList<IMappable> properties,
        IReadOnlyList<IConstructor> constructors,
        ObjectValidator? validate)
    {
        Name = name;
        Type = type;
        Properties = properties;
        Constructors = constructors;
        Validate = validate;
    }

    public IReadOnlyList<IConstructor> Constructors { get; }
    public string? Name { get; }
    public IReadOnlyList<IMappable> Properties { get; }
    public Type Type { get; }
    internal ObjectValidator? Validate { get; }
}