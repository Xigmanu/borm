using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Properties;

namespace Borm.Model.Metadata;

[DebuggerTypeProxy(typeof(EntityInfoDebugView))]
[DebuggerDisplay("Name = {Name}, DataType = {DataType.FullName}")]
internal sealed class EntityInfo
{
    private readonly ColumnInfoCollection _columns;
    private readonly string _name;

    public EntityInfo(string name, Type dataType, ColumnInfoCollection columns)
    {
        if (columns.Count == 0)
        {
            throw new ArgumentException(Strings.EmptyColumnCollection(name), nameof(columns));
        }

        _columns = columns;
        DataType = dataType;
        _name = name;
        Binding = ConversionBinding.Empty;
    }

    public Type DataType { get; }

    public string Name => _name;

    public ColumnInfo PrimaryKey
    {
        get
        {
            return _columns.FirstOrDefault(column => column.Constraints == Constraints.PrimaryKey)
                ?? throw new InvalidOperationException(Strings.MissingPrimaryKey(_name));
        }
    }

    internal ConversionBinding Binding { get; set; }
    internal ColumnInfoCollection Columns => _columns;
    internal Action<object>? Validator { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is EntityInfo other && _name == other.Name;
    }

    public override int GetHashCode()
    {
        return _name.GetHashCode();
    }

    [ExcludeFromCodeCoverage(Justification = "Debugger display proxy")]
    internal sealed class EntityInfoDebugView
    {
        private readonly EntityInfo _entityInfo;

        public EntityInfoDebugView(EntityInfo entityInfo)
        {
            _entityInfo = entityInfo;
        }

        public ColumnInfo[] Columns => [.. _entityInfo.Columns];
        public Type DataType => _entityInfo.DataType;
        public string Name => _entityInfo.Name;
        public ColumnInfo PrimaryKey => _entityInfo.PrimaryKey;
    }
}
