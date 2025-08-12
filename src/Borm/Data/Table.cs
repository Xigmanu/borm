using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

[DebuggerTypeProxy(typeof(TableDebugView))]
internal sealed class Table : ITable
{
    private readonly EntityCache _entityCache = new();
    private readonly EntityInfo _entityInfo;
    private readonly EntityMaterializer _materializer;
    private readonly ChangeTracker _tracker = new();

    public Table(EntityInfo entityInfo, IReadOnlyDictionary<IColumn, ITable> foreignKeyRelations)
    {
        _entityInfo = entityInfo;
        _materializer = new(entityInfo, foreignKeyRelations);
        ForeignKeyRelations = foreignKeyRelations;
    }

    public IEnumerable<IColumn> Columns => EntityInfo.Columns;
    public IReadOnlyDictionary<IColumn, ITable> ForeignKeyRelations { get; }
    public string Name => _entityInfo.Name;
    public IColumn PrimaryKey => EntityInfo.PrimaryKey;
    internal EntityCache EntityCache => _entityCache;
    internal EntityInfo EntityInfo => _entityInfo;
    internal EntityMaterializer Materializer => _materializer;

    public void AcceptPendingChanges(long txId)
    {
        foreach (Table dependency in ForeignKeyRelations.Values.Cast<Table>())
        {
            dependency.AcceptPendingChanges(txId);
        }
        _tracker.AcceptPendingChanges(txId);
    }

    public void Delete(object entity, long txId)
    {
        ValueBuffer buffer = _entityInfo.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();

        Change existing = GetChangeOrThrow(txId, primaryKey);

        Change change = existing.Delete(buffer, txId);
        _tracker.PendChange(change);
    }

    public override bool Equals(object? obj)
    {
        return obj is Table other && other._entityInfo.Equals(_entityInfo);
    }

    public IEnumerable<Change> GetChanges()
    {
        return _tracker.GetChanges();
    }

    public override int GetHashCode()
    {
        return _entityInfo.GetHashCode();
    }

    public void Insert(object entity, long txId)
    {
        _entityInfo.Validator?.Invoke(entity);

        ValueBuffer buffer = _entityInfo.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();

        if (_tracker.TryGetChange(primaryKey, txId, out _))
        {
            throw new ConstraintException(Strings.PrimaryKeyConstraintViolation(Name, primaryKey));
        }

        ValueBuffer resolved = ResolveForeignKeys(buffer, txId, isRecursiveInsert: true);
        Change change = Change.NewChange(resolved, txId);
        _tracker.PendChange(change);
    }

    public void MarkChangesAsWritten()
    {
        _tracker.MarkChangesAsWritten();
    }

    public IEnumerable<object> SelectAll()
    {
        IEnumerable<Change> changes = _tracker.GetChanges();
        return changes.Select(change => _materializer.FromBuffer(change.Buffer));
    }

    public void Update(object entity, long txId)
    {
        EntityInfo.Validator?.Invoke(entity);

        ValueBuffer buffer = _entityInfo.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();

        Change existing = GetChangeOrThrow(txId, primaryKey);

        ValueBuffer incoming = ResolveForeignKeys(buffer, txId, isRecursiveInsert: false);

        Change change = existing.Update(incoming, txId);
        _tracker.PendChange(change);
    }

    internal ValueBuffer GetRowByPrimaryKey(object primaryKey)
    {
        return _tracker
            .GetChanges()
            .First(change => change.Buffer.GetPrimaryKey() == primaryKey)
            .Buffer;
    }

    internal void Load(DbDataReader dataReader, long txId)
    {
        Debug.Assert(txId == InternalTransaction.InitId);
        if (!dataReader.HasRows)
        {
            return;
        }

        ColumnInfoCollection schemaColumns = _entityInfo.Columns;
        IEnumerable<string> dbColumnNames = dataReader.GetColumnSchema().Select(c => c.ColumnName);

        while (dataReader.Read())
        {
            ValueBuffer rowBuffer = new();
            foreach (string dbColumnName in dbColumnNames)
            {
                ColumnInfo schemaColumn = schemaColumns[dbColumnName]; // This might throw an exception when migrating
                rowBuffer[schemaColumn] = dataReader.GetValue(dbColumnName);
            }

            Change initChange = Change.InitChange(rowBuffer, txId);
            _tracker.PendChange(initChange);
        }
    }

    private void CheckConstraints(ColumnInfo column, object columnValue, long txId)
    {
        Constraints constraints = column.Constraints;

        if (!constraints.HasFlag(Constraints.AllowDbNull) && columnValue == null)
        {
            throw new ConstraintException(Strings.NullableConstraintViolation(column.Name, Name));
        }
        if (
            constraints.HasFlag(Constraints.Unique)
            && _tracker.TryGetChange(column, columnValue, txId, out _)
        )
        {
            throw new ConstraintException(
                Strings.UniqueConstraintViolation(Name, column.Name, columnValue)
            );
        }
    }

    private Change GetChangeOrThrow(long txId, object primaryKey)
    {
        if (_tracker.TryGetChange(primaryKey, txId, out Change? change))
        {
            return change;
        }

        throw new RowNotFoundException(Strings.RowNotFound(Name, primaryKey), Name, primaryKey);
    }

    private ValueBuffer ResolveForeignKeys(ValueBuffer incoming, long txId, bool isRecursiveInsert)
    {
        ValueBuffer result = new();

        foreach ((ColumnInfo column, object columnValue) in incoming)
        {
            CheckConstraints(column, columnValue, txId);

            if (column.Reference == null || columnValue.Equals(DBNull.Value))
            {
                result[column] = columnValue;
                continue;
            }

            Table dependencyTable = (Table)ForeignKeyRelations[column];
            EntityInfo depNode = dependencyTable.EntityInfo;

            if (column.DataType != depNode.DataType)
            {
                _ = dependencyTable.GetChangeOrThrow(txId, primaryKey: columnValue);
            }
            else
            {
                ValueBuffer fkBuffer = depNode.Binding.ToValueBuffer(columnValue);
                if (isRecursiveInsert)
                {
                    dependencyTable.Insert(fkBuffer, txId);
                }
                result[column] = fkBuffer.GetPrimaryKey();
                continue;
            }

            result[column] = columnValue;
        }

        return result;
    }

    [ExcludeFromCodeCoverage(Justification = "Debugger display proxy")]
    internal sealed class TableDebugView
    {
        private readonly Table _table;

        public TableDebugView(Table table)
        {
            _table = table;
        }

        public ChangeTracker ChangeTracker => _table._tracker;
        public IEnumerable<IColumn> Columns => _table.Columns;
        public EntityInfo EntityNode => _table._entityInfo;
        public IReadOnlyDictionary<IColumn, ITable> ForeignKeyRelations =>
            _table.ForeignKeyRelations;
        public string Name => _table.Name;
        public IColumn PrimaryKey => _table.PrimaryKey;
    }
}
