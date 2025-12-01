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

    public string MemberName { get; }
    public NullableType DataType { get; }
    public MappingInfo? Mapping { get; }
}
