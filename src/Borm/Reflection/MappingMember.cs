namespace Borm.Reflection;

public sealed record MappingMember(string MemberName, NullableType TypeInfo, MappingInfo? Mapping);
