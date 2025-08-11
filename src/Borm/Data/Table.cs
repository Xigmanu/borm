using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

[DebuggerTypeProxy(typeof(TableDebugView))]
internal sealed class Table : ITable
{
    private readonly ConstraintChecker _constraintChecker;
    private readonly EntityCache _entityCache = new();
    private readonly EntityMaterializer _materializer;
    private readonly EntityNode _node;
    private readonly ChangeTracker _tracker = new();

    public Table(EntityNode node, IReadOnlyDictionary<IColumn, ITable> foreignKeyRelations)
    {
        _node = node;
        _constraintChecker = new(_tracker, node.Name);
        _materializer = new(node, foreignKeyRelations);
        ForeignKeyRelations = foreignKeyRelations;
    }

    public IEnumerable<IColumn> Columns => Node.Columns;
    public IReadOnlyDictionary<IColumn, ITable> ForeignKeyRelations { get; }
    public string Name => _node.Name;
    public IColumn PrimaryKey => Node.PrimaryKey;
    internal EntityCache EntityCache => _entityCache;
    internal EntityMaterializer Materializer => _materializer;
    internal EntityNode Node => _node;

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
        ValueBuffer buffer = _node.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();

        AssertRowExists(txId, primaryKey);

        Change change = new(buffer, txId, RowAction.Delete);
        _tracker.PendChange(change);
    }

    public override bool Equals(object? obj)
    {
        return obj is Table other && other._node.Equals(_node);
    }

    public IEnumerable<Change> GetChanges()
    {
        return _tracker.GetChanges();
    }

    public override int GetHashCode()
    {
        return _node.GetHashCode();
    }

    public void Insert(object entity, long txId)
    {
        _node.Validator?.Invoke(entity);

        ValueBuffer buffer = _node.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();

        if (_tracker.HasChange(primaryKey, txId))
        {
            throw new ConstraintException(Strings.PrimaryKeyConstraintViolation(Name, primaryKey));
        }

        ValueBuffer resolved = ResolveForeignKeys(buffer, txId, isRecursiveInsert: true);
        Change change = new(resolved, txId, RowAction.Insert);
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
        Node.Validator?.Invoke(entity);

        ValueBuffer buffer = _node.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();

        AssertRowExists(txId, primaryKey);

        ValueBuffer incoming = ResolveForeignKeys(buffer, txId, isRecursiveInsert: false);

        Change change = new(incoming, txId, RowAction.Update);
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

        ColumnInfoCollection schemaColumns = _node.Columns;
        IEnumerable<string> dbColumnNames = dataReader.GetColumnSchema().Select(c => c.ColumnName);

        while (dataReader.Read())
        {
            ValueBuffer rowBuffer = new();
            foreach (string dbColumnName in dbColumnNames)
            {
                ColumnInfo schemaColumn = schemaColumns[dbColumnName]; // This might throw an exception when migrating
                rowBuffer[schemaColumn] = dataReader.GetValue(dbColumnName);
            }

            Change initChange = new(rowBuffer, txId, RowAction.None, isWrittenToDb: true);
            _tracker.PendChange(initChange);
        }
    }

    private void AssertRowExists(long txId, object primaryKey)
    {
        if (!_tracker.HasChange(primaryKey, txId))
        {
            throw new RowNotFoundException(
                Strings.RowNotFound(Name, primaryKey),
                Node.DataType,
                primaryKey
            );
        }
    }

    private ValueBuffer ResolveForeignKeys(ValueBuffer incoming, long txId, bool isRecursiveInsert)
    {
        ValueBuffer result = new();

        foreach ((ColumnInfo column, object columnValue) in incoming)
        {
            _constraintChecker.Check(column, columnValue, txId);

            if (column.Reference == null || columnValue.Equals(DBNull.Value))
            {
                result[column] = columnValue;
                continue;
            }

            Table dependencyTable = (Table)ForeignKeyRelations[column];
            EntityNode depNode = dependencyTable.Node;

            if (column.DataType != depNode.DataType)
            {
                dependencyTable.AssertRowExists(txId, primaryKey: columnValue);
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
        public EntityNode EntityNode => _table._node;
        public IReadOnlyDictionary<IColumn, ITable> ForeignKeyRelations =>
            _table.ForeignKeyRelations;
        public string Name => _table.Name;
        public IColumn PrimaryKey => _table.PrimaryKey;
    }
}
