namespace Borm.Reflection;

public interface IMappable
{
    string MemberName { get; }
    NullableType DataType { get; }
    MappingInfo? Mapping { get; }
    ValidationInfo? ValidationInfo { get; }
}