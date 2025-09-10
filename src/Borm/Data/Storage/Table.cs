using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Data.Sql;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Util;

namespace Borm.Data.Storage;

[DebuggerDisplay("Name = {Name}"), DebuggerTypeProxy(typeof(TableDebugView))]
internal sealed class Table
{
    private readonly ConstraintValidator _constraintValidator;
    private readonly EntityMetadata _entityMetadata;
    private readonly EntityMaterializer _materializer;
    private readonly ChangeTracker _tracker = new();

    public Table(EntityMetadata entityMetadata)
    {
        _entityMetadata = entityMetadata;
        _materializer = new(this);
        _constraintValidator = new(this);
        ParentRelations = [];
    }

    public string Name => _entityMetadata.Name;
    internal EntityMaterializer Converter => _materializer;
    internal EntityMetadata EntityMetadata => _entityMetadata;
    internal Dictionary<ColumnMetadata, Table> ParentRelations { get; }
    internal ChangeTracker Tracker => _tracker;

    public void AcceptPendingChanges(long txId)
    {
        foreach (Table dependency in ParentRelations.Values.Cast<Table>())
        {
            dependency.AcceptPendingChanges(txId);
        }
        _tracker.AcceptPendingChanges(txId);
    }

    public void Delete(ValueBuffer buffer, long txId)
    {
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

            Table parent = ParentRelations[column];
            columnInfo = new(
                columnName,
                parent._entityMetadata.PrimaryKey.DataType,
                isUnique,
                isNullable
            );
            TableInfo dependencySchema = parent.GetTableSchema();

            fkRelationMap[columnInfo] = dependencySchema;
        }

        Debug.Assert(primaryKey != null);
        return new TableInfo(_entityMetadata.Name, columns, primaryKey, fkRelationMap);
    }

    public void Insert(ValueBuffer buffer, long txId)
    {
        object primaryKey = buffer.PrimaryKey;
        if (_tracker.TryGetChange(primaryKey, txId, out _))
        {
            throw new ConstraintException(Strings.PrimaryKeyConstraintViolation(Name, primaryKey));
        }
        _constraintValidator.ValidateBuffer(buffer, txId);

        IEnumerable<EntityMaterializer.ResolvedForeignKey> resolvedKeys =
            _materializer.ResolveForeignKeys(buffer, txId);
        foreach (EntityMaterializer.ResolvedForeignKey resolvedKey in resolvedKeys)
        {
            ColumnMetadata foreignKey = resolvedKey.Column;
            if (!resolvedKey.ChangeExists)
            {
                object rawValue = resolvedKey.RawValue;

                Table parent = ParentRelations[foreignKey];
                EntityMetadata parentMetadata = parent.EntityMetadata;
                parentMetadata.Validator?.Invoke(rawValue);
                ValueBuffer parentBuffer = parentMetadata.Binding.ToValueBuffer(rawValue);

                parent.Insert(parentBuffer, txId);
            }
            buffer[foreignKey] = resolvedKey.ResolvedKey;
        }

        Change change = Change.NewChange(buffer, txId);
        _tracker.PendChange(change);
    }

    public IEnumerable<object> SelectAll()
    {
        return _tracker.Changes.Select(change => _materializer.Materialize(change.Buffer));
    }

    public void Update(ValueBuffer buffer, long txId)
    {
        object primaryKey = buffer.PrimaryKey;

        _constraintValidator.ValidateBuffer(buffer, txId);

        Change existing = GetChangeOrThrow(txId, primaryKey);

        IEnumerable<EntityMaterializer.ResolvedForeignKey> resolvedKeys =
            _materializer.ResolveForeignKeys(buffer, txId);
        foreach (EntityMaterializer.ResolvedForeignKey resolvedKey in resolvedKeys)
        {
            if (!resolvedKey.ChangeExists)
            {
                Table parent = ParentRelations[resolvedKey.Column];
                throw new RowNotFoundException(
                    Strings.RowNotFound(parent.Name, resolvedKey.ResolvedKey),
                    parent.Name,
                    resolvedKey.ResolvedKey
                );
            }
            buffer[resolvedKey.Column] = resolvedKey.ResolvedKey;
        }

        Change change = existing.Update(buffer, txId);
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
                    rowBuffer[schemaColumn] = ColumnDataTypeHelper.Parse(
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
            _table.ParentRelations;
        public string Name => _table.Name;
        public ChangeTracker Tracker => _table.Tracker;
    }
}
