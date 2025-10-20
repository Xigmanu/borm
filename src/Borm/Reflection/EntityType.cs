using System.Collections.ObjectModel;

namespace Borm.Reflection;

internal sealed record EntityType(
    string? Name,
    Type Type,
    ReadOnlyCollection<MappingMember> Properties,
    ReadOnlyCollection<Constructor> Constructors
);
