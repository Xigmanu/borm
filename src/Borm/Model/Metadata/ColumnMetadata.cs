using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Borm.Model.Metadata;

[DebuggerTypeProxy(typeof(ColumnMetadataDebugView))]
[DebuggerDisplay("Name = {Name}, DataType = {DataType.FullName}")]
internal sealed class ColumnMetadata
{
    public ColumnMetadata(
        int index,
        string columnName,
        string propertyName,
        Type propertyType,
        Constraints constraints
    )
    {
        Index = index;
        Name = columnName;
        DataType =
            propertyType.IsValueType && constraints.HasFlag(Constraints.AllowDbNull)
                ? Nullable.GetUnderlyingType(propertyType)!
                : propertyType;
        Constraints = constraints;
        PropertyName = propertyName;
        PropertyType = propertyType;
    }

    public Constraints Constraints { get; }
    public Type DataType { get; }
    public int Index { get; }
    public string Name { get; }
    public ReferentialAction OnDelete { get; internal set; }
    public ReferentialAction OnUpdate { get; internal set; }
    public string PropertyName { get; }
    public Type PropertyType { get; }
    public Type? Reference { get; internal set; }

    [ExcludeFromCodeCoverage(Justification = "Debug display proxy")]
    internal sealed class ColumnMetadataDebugView
    {
        private readonly ColumnMetadata _metadata;

        public ColumnMetadataDebugView(ColumnMetadata metadata)
        {
            _metadata = metadata;
        }

        public Constraints Constraints => _metadata.Constraints;
        public Type DataType => _metadata.DataType;
        public int Index => _metadata.Index;
        public string Name => _metadata.Name;
        public Type? Reference => _metadata.Reference;
    }
}
