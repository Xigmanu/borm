using Borm.Reflection;

namespace Borm.Model.Metadata;

internal interface IColumnMetadata
{
    public Constraints Constraints { get; }
    public NullableType DataType { get; }
    public int Index { get; }
    public string Name { get; }
    public ReferentialAction OnDelete { get; }
    public string PropertyName { get; }
    public Type? Reference { get; }
}
