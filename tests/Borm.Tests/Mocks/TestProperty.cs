using Borm.Reflection;

namespace Borm.Tests.Mocks;

internal sealed class TestProperty : IMappable
{
    public TestProperty(string memberName, NullableType dataType, MappingInfo? mapping)
    {
        MemberName = memberName;
        DataType = dataType;
        Mapping = mapping;
    }

    public TestProperty(
        string memberName,
        NullableType dataType,
        MappingInfo? mapping,
        ValidationInfo? validationInfo
    )
        : this(memberName, dataType, mapping)
    {
        ValidationInfo = validationInfo;
    }

    public NullableType DataType { get; }
    public MappingInfo? Mapping { get; }
    public string MemberName { get; }
    public ValidationInfo? ValidationInfo { get; }
}