namespace Borm.Reflection.Internal;

internal sealed class Parameter : IMappable
{
    public Parameter(string memberName, NullableType dataType)
    {
        MemberName = memberName;
        DataType = dataType;
        Mapping = null;
    }

    public NullableType DataType { get; }
    public MappingInfo? Mapping { get; }
    public string MemberName { get; }
}