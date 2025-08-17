using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

[DebuggerTypeProxy(typeof(TableDebugView))]
internal sealed class Table
{
    private readonly EntityConverter _converter;
    private readonly EntityMetadata _entityMetadata;
    private readonly Dictionary<ColumnMetadata, Table> _foreignKeyRelations;
    private readonly ChangeTracker _tracker = new();

    public Table(
        EntityMetadata entityMetadata,
        Dictionary<ColumnMetadata, Table> foreignKeyRelations
    )
    {
        _entityMetadata = entityMetadata;
        _foreignKeyRelations = foreignKeyRelations;
        _converter = new(this);
    }

    public string Name => _entityMetadata.Name;
    internal EntityConverter Converter => _converter;
    internal EntityMetadata EntityMetadata => _entityMetadata;
    internal IReadOnlyDictionary<ColumnMetadata, Table> ForeignKeyRelations => _foreignKeyRelations;
    internal ChangeTracker Tracker => _tracker;

    public void AcceptPendingChanges(long txId)
    {
        foreach (Table dependency in _foreignKeyRelations.Values.Cast<Table>())
        {
            dependency.AcceptPendingChanges(txId);
        }
        _tracker.AcceptPendingChanges(txId);
    }

    public void Delete(object entity, long txId)
    {
        ValueBuffer buffer = _entityMetadata.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.PrimaryKey;

        Change existing = GetChangeOrThrow(txId, primaryKey);

        Change change = existing.Delete(buffer, txId);
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

    public TableInfo GetTableSchema()
    {
        List<ColumnInfo> columns = [];
        Dictionary<ColumnInfo, TableInfo> fkRelationMap = [];

        ColumnInfo? primaryKey = null;
        foreach (ColumnMetadata column in _entityMetadata.Columns)
        {
            string columnName = column.Name;
            bool isUnique = column.Constraints.HasFlag(Constraints.Unique);
            bool isNullable = column.Constraints.HasFlag(Constraints.AllowDbNull);

            ColumnInfo columnInfo;
            if (column.Reference == null)
            {
                columnInfo = new(columnName, column.DataType, isUnique, isNullable);
                columns.Add(columnInfo);

                if (column.Constraints.HasFlag(Constraints.PrimaryKey))
                {
                    primaryKey = columnInfo;
                }
                continue;
            }

            Table dependency = _foreignKeyRelations[column];
            columnInfo = new(
                columnName,
                dependency._entityMetadata.PrimaryKey.DataType,
                isUnique,
                isNullable
            );
            TableInfo dependencySchema = dependency.GetTableSchema();

            fkRelationMap[columnInfo] = dependencySchema;
        }

        Debug.Assert(primaryKey != null);
        return new TableInfo(_entityMetadata.Name, columns, primaryKey, fkRelationMap);
    }

    public void Insert(object entity, long txId)
    {
        _entityMetadata.Validator?.Invoke(entity);

        ValueBuffer buffer = _entityMetadata.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.PrimaryKey;

        if (_tracker.TryGetChange(primaryKey, txId, out _))
        {
            throw new ConstraintException(Strings.PrimaryKeyConstraintViolation(Name, primaryKey));
        }

        ValueBuffer resolved = _converter.ResolveForeignKeyValues(
            buffer,
            txId,
            isRecursiveInsert: true
        );
        Change change = Change.NewChange(resolved, txId);
        _tracker.PendChange(change);
    }

    public IEnumerable<object> SelectAll()
    {
        IEnumerable<Change> changes = _tracker.GetChanges();
        return changes.Select(change => _converter.Materialize(change.Buffer));
    }

    public void Update(object entity, long txId)
    {
        EntityMetadata.Validator?.Invoke(entity);

        ValueBuffer buffer = _entityMetadata.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.PrimaryKey;

        Change existing = GetChangeOrThrow(txId, primaryKey);

        ValueBuffer incoming = _converter.ResolveForeignKeyValues(
            buffer,
            txId,
            isRecursiveInsert: false
        );

        Change change = existing.Update(incoming, txId);
        _tracker.PendChange(change);
    }

    internal void Load(DbDataReader dataReader, long txId)
    {
        Debug.Assert(txId == InternalTransaction.InitId);
        if (!dataReader.HasRows)
        {
            return;
        }

        ColumnMetadataCollection schemaColumns = _entityMetadata.Columns;
        IEnumerable<string> dbColumnNames = dataReader.GetColumnSchema().Select(c => c.ColumnName);

        while (dataReader.Read())
        {
            ValueBuffer rowBuffer = new();
            foreach (string dbColumnName in dbColumnNames)
            {
                ColumnMetadata schemaColumn = schemaColumns[dbColumnName]; // This might throw an exception when migrating
                rowBuffer[schemaColumn] = dataReader.GetValue(dbColumnName);
            }

            Change initChange = Change.Initial(rowBuffer, txId);
            _tracker.PendChange(initChange);
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

    [ExcludeFromCodeCoverage(Justification = "Debug display proxy")]
    internal sealed class TableDebugView
    {
        private readonly Table _table;

        public TableDebugView(Table table)
        {
            _table = table;
        }

        public EntityMetadata EntityMetadata => _table.EntityMetadata;
        public IReadOnlyDictionary<ColumnMetadata, Table> ForeignKeyRelations =>
            _table.ForeignKeyRelations;
        public string Name => _table.Name;
        public ChangeTracker Tracker => _table.Tracker;
    }
}
