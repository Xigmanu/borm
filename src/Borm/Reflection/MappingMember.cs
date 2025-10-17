namespace Borm.Reflection;

internal sealed record MappingMember(string MemberName, TypeInfo TypeInfo, MappingInfo? Mapping);
