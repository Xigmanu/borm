using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Data.Sql;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data.Storage;

[DebuggerDisplay("Name = {Name}"), DebuggerTypeProxy(typeof(TableDebugView))]
internal sealed class Table
{
    private readonly ConstraintValidator _constraintValidator;
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
        _materializer = new(this);
        _constraintValidator = new(this);
    }

    public string Name => _entityMetadata.Name;
    internal EntityMaterializer Converter => _materializer;
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
        Debug.Assert(entity.GetType().Equals(_entityMetadata.DataType));

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
        Debug.Assert(entity.GetType().Equals(_entityMetadata.DataType));

        _entityMetadata.Validator?.Invoke(entity);

        ValueBuffer incoming = _entityMetadata.Binding.ToValueBuffer(entity);
        object primaryKey = incoming.PrimaryKey;
        if (_tracker.TryGetChange(primaryKey, txId, out _))
        {
            throw new ConstraintException(Strings.PrimaryKeyConstraintViolation(Name, primaryKey));
        }
        _constraintValidator.ValidateBuffer(incoming, txId);

        IEnumerable<EntityMaterializer.ResolvedForeignKey> resolvedKeys =
            _materializer.ResolveForeignKeys(incoming, txId);
        foreach (EntityMaterializer.ResolvedForeignKey resolvedKey in resolvedKeys)
        {
            if (!resolvedKey.ChangeExists)
            {
                ForeignKeyRelations[resolvedKey.Column].Insert(resolvedKey.RawValue, txId);
            }
            incoming[resolvedKey.Column] = resolvedKey.ResolvedKey;
        }

        Change change = Change.NewChange(incoming, txId);
        _tracker.PendChange(change);
    }

    public IEnumerable<object> SelectAll()
    {
        return _tracker.Changes.Select(change => _materializer.Materialize(change.Buffer));
    }

    public void Update(object entity, long txId)
    {
        Debug.Assert(entity.GetType().Equals(_entityMetadata.DataType));

        _entityMetadata.Validator?.Invoke(entity);

        ValueBuffer incoming = _entityMetadata.Binding.ToValueBuffer(entity);
        object primaryKey = incoming.PrimaryKey;

        _constraintValidator.ValidateBuffer(incoming, txId);

        Change existing = GetChangeOrThrow(txId, primaryKey);

        IEnumerable<EntityMaterializer.ResolvedForeignKey> resolvedKeys =
            _materializer.ResolveForeignKeys(incoming, txId);
        foreach (EntityMaterializer.ResolvedForeignKey resolvedKey in resolvedKeys)
        {
            incoming[resolvedKey.Column] = resolvedKey.ResolvedKey;
        }

        Change change = existing.Update(incoming, txId);
        _tracker.PendChange(change);
    }

    internal void Load(ResultSet resultSet, long txId)
    {
        Debug.Assert(txId == InternalTransaction.InitId);
        if (resultSet.RowCount == 0)
        {
            return;
        }

        ColumnMetadataCollection schemaColumns = _entityMetadata.Columns;

        while (resultSet.MoveNext())
        {
            ValueBuffer rowBuffer = new();
            foreach ((string columnName, object columnValue) in resultSet.Current)
            {
                ColumnMetadata schemaColumn = schemaColumns[columnName]; // This might throw an exception when migrating
                if (columnValue is string columnValueStr)
                {
                    rowBuffer[schemaColumn] = TypeParser.Parse(
                        columnValueStr,
                        schemaColumn.DataType
                    );
                }
                else
                {
                    rowBuffer[schemaColumn] = columnValue;
                }
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
