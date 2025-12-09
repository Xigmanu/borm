using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Borm.Data.Sql;
using Borm.Data.Storage.Tracking;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Util;

namespace Borm.Data.Storage.Internal;

[DebuggerDisplay("Name = {Name}")]
[DebuggerTypeProxy(typeof(TableDebugView))]
internal sealed class Table : ITable
{
    private readonly ConstraintValidator _constraintValidator;

    public Table(IEntityMetadata entityMetadata)
    {
        Metadata = entityMetadata;
        _constraintValidator = new ConstraintValidator(this);
    }

    public IEntityMetadata Metadata { get; }
    public string Name => Metadata.Name;
    public ChangeTracker Tracker { get; } = new();

    public void Delete(IValueBuffer record, long txId)
    {
        AssertBufferValuesAreSimple(record);

        object primaryKey = record.PrimaryKey;

        IChange existing = GetChangeOrThrow(txId, primaryKey);

        IChange change = ChangeFactory.Delete(existing, record, txId);
        Tracker.PendChange(change);
    }

    public void Insert(IValueBuffer record, long txId)
    {
        AssertBufferValuesAreSimple(record);

        object primaryKey = record.PrimaryKey;
        if (Tracker.TryGetChange(primaryKey, txId, out _))
        {
            throw new ConstraintException(Strings.PrimaryKeyConstraintViolation(Name, primaryKey));
        }

        _constraintValidator.ValidateBuffer(record, txId);

        IChange change = ChangeFactory.NewChange(record, txId);
        Tracker.PendChange(change);
    }

    public void Load(ResultSet resultSet, long txId)
    {
        Debug.Assert(txId == Transaction.InitId);
        if (resultSet.RowCount == 0)
        {
            return;
        }

        IReadOnlyCollection<IColumnMetadata> schemaColumns = Metadata.Columns;

        while (resultSet.MoveNext())
        {
            ValueBuffer rowBuffer = new();
            foreach ((string columnName, object columnValue) in resultSet.Current)
            {
                IColumnMetadata
                    schemaColumn =
                        schemaColumns.First(col =>
                            col.Name == columnName); // This might throw an exception when migrating
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
            Tracker.PendChange(initChange);
        }
    }

    public void Update(IValueBuffer record, long txId)
    {
        AssertBufferValuesAreSimple(record);

        object primaryKey = record.PrimaryKey;

        _constraintValidator.ValidateBuffer(record, txId);

        IChange existing = GetChangeOrThrow(txId, primaryKey);

        IChange change = ChangeFactory.Update(existing, record, txId);
        Tracker.PendChange(change);
    }

    public override bool Equals(object? obj)
    {
        return obj is Table other && other.Metadata.Equals(Metadata);
    }

    public override int GetHashCode()
    {
        return Metadata.GetHashCode();
    }

    [Conditional("DEBUG")]
    [ExcludeFromCodeCoverage]
    private void AssertBufferValuesAreSimple(
        IValueBuffer record,
        [CallerMemberName] string? callerName = null
    )
    {
        const string messageFormat =
            "Incoming record contains illegal values. Table: '{0}', Column: '{1}', Value: '{2}', Operation: '{3}'";
        foreach ((IColumnMetadata column, object value) in record)
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
            Tracker.TryGetChange(primaryKey, txId, out IChange? change)
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