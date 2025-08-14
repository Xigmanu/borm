using System.Data;
using System.Data.Common;
using System.Diagnostics;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

internal sealed class Table
{
    private readonly EntityCache _entityCache = new();
    private readonly EntityMetadata _entityMetadata;
    private readonly Dictionary<ColumnMetadata, Table> _foreignKeyRelations;
    private readonly EntityMaterializer _materializer;
    private readonly ChangeTracker _tracker = new();

    public Table(
        EntityMetadata entityMetadata,
        Dictionary<ColumnMetadata, Table> foreignKeyRelations
    )
    {
        _entityMetadata = entityMetadata;
        _foreignKeyRelations = foreignKeyRelations;
        _materializer = new(entityMetadata, foreignKeyRelations);
    }

    public string Name => _entityMetadata.Name;
    internal EntityCache EntityCache => _entityCache;
    internal EntityMetadata EntityInfo => _entityMetadata;
    internal IReadOnlyDictionary<ColumnMetadata, Table> ForeignKeyRelations => _foreignKeyRelations;
    internal EntityMaterializer Materializer => _materializer;

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

    public IEnumerable<Change> GetChanges()
    {
        return _tracker.GetChanges();
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

        ValueBuffer buffer = _entityMetadata.Binding.ToValueBuffer(entity);
        object primaryKey = buffer.PrimaryKey;

        Change existing = GetChangeOrThrow(txId, primaryKey);

        ValueBuffer incoming = ResolveForeignKeys(buffer, txId, isRecursiveInsert: false);

        Change change = existing.Update(incoming, txId);
        _tracker.PendChange(change);
    }

    internal ValueBuffer GetRowByPrimaryKey(object primaryKey)
    {
        return _tracker
            .GetChanges()
            .First(change => change.Buffer.PrimaryKey.Equals(primaryKey))
            .Buffer;
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

            Change initChange = Change.InitChange(rowBuffer, txId);
            _tracker.PendChange(initChange);
        }
    }

    private void CheckConstraints(ColumnMetadata column, object columnValue, long txId)
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

        foreach ((ColumnMetadata column, object columnValue) in incoming)
        {
            CheckConstraints(column, columnValue, txId);

            if (column.Reference == null || columnValue.Equals(DBNull.Value))
            {
                result[column] = columnValue;
                continue;
            }

            Table dependencyTable = _foreignKeyRelations[column];
            EntityMetadata depNode = dependencyTable.EntityInfo;

            if (column.DataType != depNode.DataType)
            {
                _ = dependencyTable.GetChangeOrThrow(txId, primaryKey: columnValue);
            }
            else
            {
                ValueBuffer fkBuffer = depNode.Binding.ToValueBuffer(columnValue);
                object depPrimaryKey = fkBuffer.PrimaryKey;

                bool changeExists = dependencyTable._tracker.TryGetChange(
                    depPrimaryKey,
                    txId,
                    out _
                );
                if (isRecursiveInsert && !changeExists)
                {
                    dependencyTable.Insert(columnValue, txId);
                }

                result[column] = depPrimaryKey;
                continue;
            }

            result[column] = columnValue;
        }

        return result;
    }
}
