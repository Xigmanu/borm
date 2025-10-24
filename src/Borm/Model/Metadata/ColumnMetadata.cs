using Borm.Reflection;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Borm.Model.Metadata;

[DebuggerTypeProxy(typeof(ColumnMetadataDebugView))]
[DebuggerDisplay("Name = {Name}, DataType = {DataType}")]
internal sealed class ColumnMetadata : IColumnMetadata
{
    public ColumnMetadata(
        int index,
        string columnName,
        string propertyName,
        NullableType dataType,
        Constraints constraints
    )
    {
        Index = index;
        Name = columnName;
        DataType = dataType;
        Constraints = constraints;
        PropertyName = propertyName;
    }

    public Constraints Constraints { get; }
    public NullableType DataType { get; }
    public int Index { get; }
    public string Name { get; }
    public ReferentialAction OnDelete { get; internal set; }
    public string PropertyName { get; }
    public Type? Reference { get; internal set; }

    public override bool Equals(object? obj)
    {
        return obj is ColumnMetadata other && Name == other.Name && Index == other.Index;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Index);
    }

    [ExcludeFromCodeCoverage(Justification = "Debug display proxy")]
    internal sealed class ColumnMetadataDebugView
    {
        private readonly ColumnMetadata _metadata;

        public ColumnMetadataDebugView(ColumnMetadata metadata)
        {
            _metadata = metadata;
        }

        public Constraints Constraints => _metadata.Constraints;
        public NullableType DataType => _metadata.DataType;
        public int Index => _metadata.Index;
        public string Name => _metadata.Name;
        public Type? Reference => _metadata.Reference;
    }
}
