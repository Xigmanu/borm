namespace Borm.Reflection;

internal sealed record EntityTypeInfo(
    string? Name,
    Type Type,
    IReadOnlyList<MappingMember> Properties,
    IReadOnlyList<Constructor> Constructors,
    Action<object>? Validate
);
