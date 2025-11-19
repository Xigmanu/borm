using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Model.Conversion;
using Borm.Model.Validation;
using Borm.Properties;

namespace Borm.Model.Metadata;

[DebuggerTypeProxy(typeof(EntityInfoDebugView))]
[DebuggerDisplay("Name = {Name}, Type = {Type}")]
internal sealed class EntityMetadata : IEntityMetadata
{
    private readonly ValidatorFunc? _validate;

    public EntityMetadata(string name, Type dataType, IReadOnlyList<IColumnMetadata> columns)
        : this(name, dataType, columns, EntityBufferConversion.Empty, null)
    {
    }

    public EntityMetadata(
        string name,
        Type dataType,
        IReadOnlyList<IColumnMetadata> columns,
        IEntityBufferConversion conversion,
        ValidatorFunc? validate
    )
    {
        if (columns.Count == 0)
        {
            throw new ArgumentException(Strings.EmptyColumnCollection(name), nameof(columns));
        }

        Columns = columns;
        Name = name;
        _validate = validate;
        Type = dataType;
        Conversion = conversion;
    }

    public IReadOnlyList<IColumnMetadata> Columns { get; }

    public IEntityBufferConversion Conversion { get; }
    public string Name { get; }

    public IColumnMetadata PrimaryKey
    {
        get
        {
            return Columns.FirstOrDefault(column => column.Constraints == Constraints.PrimaryKey)
                   ?? throw new InvalidOperationException(Strings.MissingPrimaryKey(Name));
        }
    }

    public Type Type { get; }

    public void Validate(object entity)
    {
        ValidationResult? result = _validate?.Invoke(entity);
        if (result is { IsError: true })
        {
            throw new InvalidObjectException(result.Value);
        }
    }

    public override bool Equals(object? obj)
    {
        return obj is EntityMetadata other && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

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