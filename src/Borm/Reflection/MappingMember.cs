namespace Borm.Reflection;

internal sealed record MappingMember(string MemberName, NullableType TypeInfo, MappingInfo? Mapping);
