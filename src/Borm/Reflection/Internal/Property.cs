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
        ValidationInfo = validation;
    }

    public NullableType DataType { get; }
    public MappingInfo Mapping { get; }
    public string MemberName { get; }
    public ValidationInfo? ValidationInfo { get; }
}
