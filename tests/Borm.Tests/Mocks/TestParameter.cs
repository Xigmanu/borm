using Borm.Reflection;

namespace Borm.Tests.Mocks;

internal sealed class TestParameter : IMappable
{
    public TestParameter(string memberName, NullableType dataType)
    {
        MemberName = memberName;
        DataType = dataType;
        Mapping = null;
        ValidationInfo = null;
    }

    public string MemberName { get; }
    public NullableType DataType { get; }
    public MappingInfo? Mapping { get; }
    public ValidationInfo? ValidationInfo { get; }
}