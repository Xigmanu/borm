using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Borm.Data.Sql;
using Borm.Data.Storage.Tracking;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Util;

namespace Borm.Data.Storage;

[DebuggerDisplay("Name = {Name}"), DebuggerTypeProxy(typeof(TableDebugView))]
internal sealed class Table
{
    private readonly ConstraintValidator _constraintValidator;
    private readonly IEntityMetadata _entityMetadata;
    private readonly ChangeTracker _tracker = new();

    public Table(IEntityMetadata entityMetadata)
    {
        _entityMetadata = entityMetadata;
        _constraintValidator = new(this);
    }

    public string Name => _entityMetadata.Name;
    internal IEntityMetadata Metadata => _entityMetadata;
    internal ChangeTracker Tracker => _tracker;

    public void Delete(IValueBuffer buffer, long txId)
    {
        AssertBufferValuesAreSimple(buffer);

        object primaryKey = buffer.PrimaryKey;

        IChange existing = GetChangeOrThrow(txId, primaryKey);

        IChange change = ChangeFactory.Delete(existing, buffer, txId);
        _tracker.PendChange(change);
    }

    public override bool Equals(object? obj)
    {
        return obj is Table other && other._entityMetadata.Equals(_entityMetadata);
    }

    public override int GetHashCode()
    {
        return _entityMetadata.GetHashCode();
    }

    public void Insert(IValueBuffer buffer, long txId)
    {
        AssertBufferValuesAreSimple(buffer);

        object primaryKey = buffer.PrimaryKey;
        if (_tracker.TryGetChange(primaryKey, txId, out _))
        {
            throw new ConstraintException(Strings.PrimaryKeyConstraintViolation(Name, primaryKey));
        }
        _constraintValidator.ValidateBuffer(buffer, txId);

        IChange change = ChangeFactory.NewChange(buffer, txId);
        _tracker.PendChange(change);
    }

    public void Update(IValueBuffer buffer, long txId)
    {
        AssertBufferValuesAreSimple(buffer);

        object primaryKey = buffer.PrimaryKey;

        _constraintValidator.ValidateBuffer(buffer, txId);

        IChange existing = GetChangeOrThrow(txId, primaryKey);

        IChange change = ChangeFactory.Update(existing, buffer, txId);
        _tracker.PendChange(change);
    }

    internal void Load(ResultSet resultSet, long txId)
    {
        Debug.Assert(txId == Transaction.InitId);
        if (resultSet.RowCount == 0)
        {
            return;
        }

        IReadOnlyCollection<IColumnMetadata> schemaColumns = _entityMetadata.Columns;

        while (resultSet.MoveNext())
        {
            ValueBuffer rowBuffer = new();
            foreach ((string columnName, object columnValue) in resultSet.Current)
            {
                IColumnMetadata schemaColumn = schemaColumns.First(col => col.Name == columnName); // This might throw an exception when migrating
                if (columnValue is string columnValueStr)
                {
                    rowBuffer[schemaColumn] = ColumnDataTypeHelper.Parse(
                        columnValueStr,
                        schemaColumn.DataType.UnderlyingType
                    );
                }
                else
                {
                    rowBuffer[schemaColumn] = columnValue;
                }
            }

            IChange initChange = ChangeFactory.Initial(rowBuffer, txId);
            _tracker.PendChange(initChange);
        }
    }

    [Conditional("DEBUG")]
    [ExcludeFromCodeCoverage]
    private void AssertBufferValuesAreSimple(
        IValueBuffer buffer,
        [CallerMemberName] string? callerName = null
    )
    {
        const string messageFormat =
            "Incoming buffer contains illegal values. Table: '{0}', Column: '{1}', Value: '{2}', Operation: '{3}'";
        foreach ((IColumnMetadata column, object value) in buffer)
        {
            Debug.Assert(
                ColumnDataTypeHelper.IsSupported(value.GetType()) || value == DBNull.Value,
                string.Format(messageFormat, Name, column.Name, value, callerName)
            );
        }
    }

    private IChange GetChangeOrThrow(long txId, object primaryKey)
    {
        if (
            _tracker.TryGetChange(primaryKey, txId, out IChange? change)
            && change.RowAction != RowAction.Delete
        )
        {
            return change;
        }

        throw new RecordNotFoundException(Strings.RowNotFound(Name, primaryKey), Name, primaryKey);
    }

    [ExcludeFromCodeCoverage(Justification = "Debug display proxy")]
    internal sealed class TableDebugView
    {
        private readonly Table _table;

        public TableDebugView(Table table)
        {
            _table = table;
        }

        public IEntityMetadata EntityMetadata => _table.Metadata;
        public string Name => _table.Name;
        public ChangeTracker Tracker => _table.Tracker;
    }
}
