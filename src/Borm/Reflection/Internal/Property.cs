namespace Borm.Reflection.Internal;

internal sealed class Property : IMappable
{
    public Property(
        string memberName,
        NullableType dataType,
        MappingInfo mapping,
        ValidationInfo? validation
    )
    {
        MemberName = memberName;
        DataType = dataType;
        Mapping = mapping;
        Validation = validation;
    }

    internal ValidationInfo? Validation { get; }

    public NullableType DataType { get; }
    public MappingInfo Mapping { get; }
    public string MemberName { get; }
}