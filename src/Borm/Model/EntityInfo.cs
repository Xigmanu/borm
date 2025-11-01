using Borm.Reflection;

namespace Borm.Model;

public sealed record EntityInfo(
    string? Name,
    Type Type,
    IReadOnlyList<MappingMember> Properties,
    IReadOnlyList<Constructor> Constructors,
    Action<object>? Validate
);
