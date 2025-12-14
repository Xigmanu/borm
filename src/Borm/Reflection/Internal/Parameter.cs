namespace Borm.Reflection.Internal;

internal sealed class Parameter : IMappable
{
    public Parameter(string memberName, NullableType dataType)
    {
        MemberName = memberName;
        DataType = dataType;
    }

    public NullableType DataType { get; }
    public MappingInfo? Mapping => null;
    public string MemberName { get; }
    public ValidationInfo? ValidationInfo => null;
}