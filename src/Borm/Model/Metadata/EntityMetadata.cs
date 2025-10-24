using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Model.Metadata.Conversion;
using Borm.Properties;

namespace Borm.Model.Metadata;

[DebuggerTypeProxy(typeof(EntityInfoDebugView))]
[DebuggerDisplay("Name = {Name}, Type = {Type.FullName}")]
internal sealed class EntityMetadata : IEntityMetadata
{
    private readonly IReadOnlyList<IColumnMetadata> _columns;
    private readonly string _name;
    private readonly Action<object>? _validate;

    public EntityMetadata(string name, Type dataType, IReadOnlyList<IColumnMetadata> columns)
        : this(name, dataType, columns, EntityBufferConversion.Empty, null) { }

    public EntityMetadata(
        string name,
        Type dataType,
        IReadOnlyList<IColumnMetadata> columns,
        IEntityBufferConversion conversion,
        Action<object>? validate
    )
    {
        if (columns.Count == 0)
        {
            throw new ArgumentException(Strings.EmptyColumnCollection(name), nameof(columns));
        }

        _columns = columns;
        _name = name;
        _validate = validate;
        Type = dataType;
        Conversion = conversion;
    }

    public IReadOnlyList<IColumnMetadata> Columns => _columns;
    public IEntityBufferConversion Conversion { get; }
    public string Name => _name;
    public IColumnMetadata PrimaryKey
    {
        get
        {
            return _columns.FirstOrDefault(column => column.Constraints == Constraints.PrimaryKey)
                ?? throw new InvalidOperationException(Strings.MissingPrimaryKey(_name));
        }
    }

    public Type Type { get; }

    public override bool Equals(object? obj)
    {
        return obj is EntityMetadata other && _name == other.Name;
    }

    public override int GetHashCode()
    {
        return _name.GetHashCode();
    }

    public void Validate(object entity) => _validate?.Invoke(entity);

    [ExcludeFromCodeCoverage(Justification = "Debugger display proxy")]
    internal sealed class EntityInfoDebugView
    {
        private readonly EntityMetadata _entityMetadata;

        public EntityInfoDebugView(EntityMetadata entityMetadata)
        {
            _entityMetadata = entityMetadata;
        }

        public IColumnMetadata[] Columns => [.. _entityMetadata.Columns];
        public Type DataType => _entityMetadata.Type;
        public string Name => _entityMetadata.Name;
        public IColumnMetadata PrimaryKey => _entityMetadata.PrimaryKey;
    }
}
