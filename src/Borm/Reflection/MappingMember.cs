namespace Borm.Reflection;

public sealed class MappingMember
{
    internal MappingMember(
        string memberName,
        NullableType type,
        MappingInfo? mapping,
        ValidatorExpressionInfo? validation
    )
    {
        MemberName = memberName;
        Type = type;
        Mapping = mapping;
        Validation = validation;
    }

    public MappingInfo? Mapping { get; }
    public string MemberName { get; }
    public NullableType Type { get; }
    internal ValidatorExpressionInfo? Validation { get; }
}