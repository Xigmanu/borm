using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Properties;

namespace Borm.Model.Metadata;

[DebuggerTypeProxy(typeof(EntityInfoDebugView))]
[DebuggerDisplay("Name = {Name}, DataType = {DataType.FullName}")]
internal sealed class EntityMetadata
{
    private readonly ColumnMetadataList _columns;
    private readonly string _name;

    public EntityMetadata(string name, Type dataType, ColumnMetadataList columns)
    {
        if (columns.Count == 0)
        {
            throw new ArgumentException(Strings.EmptyColumnCollection(name), nameof(columns));
        }

        _columns = columns;
        DataType = dataType;
        _name = name;
        Binding = EntityConversionBinding.Empty;
    }

    public Type DataType { get; }

    public string Name => _name;

    public ColumnMetadata PrimaryKey
    {
        get
        {
            return _columns.FirstOrDefault(column => column.Constraints == Constraints.PrimaryKey)
                ?? throw new InvalidOperationException(Strings.MissingPrimaryKey(_name));
        }
    }

    internal EntityConversionBinding Binding { get; set; }
    internal IReadOnlyList<ColumnMetadata> Columns => _columns;
    internal Action<object>? Validator { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is EntityMetadata other && _name == other.Name;
    }

    public override int GetHashCode()
    {
        return _name.GetHashCode();
    }

    [ExcludeFromCodeCoverage(Justification = "Debugger display proxy")]
    internal sealed class EntityInfoDebugView
    {
        private readonly EntityMetadata _entityMetadata;

        public EntityInfoDebugView(EntityMetadata entityMetadata)
        {
            _entityMetadata = entityMetadata;
        }

        public ColumnMetadata[] Columns => [.. _entityMetadata.Columns];
        public Type DataType => _entityMetadata.DataType;
        public string Name => _entityMetadata.Name;
        public ColumnMetadata PrimaryKey => _entityMetadata.PrimaryKey;
    }
}
