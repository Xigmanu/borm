using Borm.Model.Validation;
using Borm.Reflection;

namespace Borm.Model;

public sealed class EntityInfo
{
    internal EntityInfo(string? name,
        Type type,
        IReadOnlyList<MappingMember> properties,
        IReadOnlyList<Constructor> constructors,
        ObjectValidator? validate)
    {
        Name = name;
        Type = type;
        Properties = properties;
        Constructors = constructors;
        Validate = validate;
    }

    public IReadOnlyList<Constructor> Constructors { get; }
    public string? Name { get; }
    public IReadOnlyList<MappingMember> Properties { get; }
    public Type Type { get; }
    internal ObjectValidator? Validate { get; }
}