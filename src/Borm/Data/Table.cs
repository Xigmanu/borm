using System.Data;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

internal sealed class Table : ITable
{
    private readonly ObjectCache _entityCache = new();
    private readonly EntityNode _node;
    private readonly ChangeTracker _tracker = new();

    public Table(EntityNode node, IReadOnlyDictionary<IColumn, ITable> relations)
    {
        _node = node;
        Relations = relations;
    }

    public IEnumerable<IColumn> Columns => Node.Columns;

    public string Name => _node.Name;

    public IColumn PrimaryKey => Node.GetPrimaryKey();

    public IReadOnlyDictionary<IColumn, ITable> Relations { get; }

    internal ObjectCache EntityCache => _entityCache;

    internal EntityNode Node => _node;

    public void AcceptPendingChanges(long txId)
    {
        foreach (Table dependency in Relations.Values.Cast<Table>())
        {
            dependency.AcceptPendingChanges(txId);
        }
        _tracker.AcceptPendingChanges(txId);
    }

    public void MarkChangesAsWritten()
    {
        _tracker.MarkChangesAsWritten();
    }

    public void Delete(object entity, long txId)
    {
        ValueBuffer buffer = _node.Binding.ConvertToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();
        if (!_tracker.HasChange(primaryKey, txId))
        {
            throw new RowNotFoundException(
                Strings.RowNotFound(Name, primaryKey),
                Node.DataType,
                primaryKey
            );
        }

        Change change = new(buffer, txId, RowAction.Delete);
        _tracker.PendChange(change, txId);
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
        Node.Validator?.Invoke(entity);

        ValueBuffer buffer = _node.Binding.ConvertToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();
        if (_tracker.HasChange(primaryKey, txId))
        {
            throw new ConstraintException(Strings.PrimaryKeyConstraintViolation(Name, primaryKey));
        }

        ValueBuffer incoming = ResolveColumnValues(buffer, txId, isRecursiveInsert: true);

        Change change = new(incoming, txId, RowAction.Insert);
        _tracker.PendChange(change, txId);
    }

    public IEnumerable<object> SelectAll()
    {
        IEnumerable<Change> changes = _tracker.GetChanges();
        return changes.Select(change => SelectByBuffer(change.Buffer));
    }

    public void Update(object entity, long txId)
    {
        Node.Validator?.Invoke(entity);

        ValueBuffer buffer = _node.Binding.ConvertToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();
        if (!_tracker.HasChange(primaryKey, txId))
        {
            throw new RowNotFoundException(
                Strings.RowNotFound(Name, primaryKey),
                Node.DataType,
                primaryKey
            );
        }

        ValueBuffer incoming = ResolveColumnValues(buffer, txId, isRecursiveInsert: false);

        Change change = new(incoming, txId, RowAction.Update);
        _tracker.PendChange(change, txId);
    }

    internal void Load(IDataReader dataReader)
    {
        throw new NotImplementedException();
    }

    private void CheckConstraints(ColumnInfo column, object columnValue, long txId)
    {
        Constraints constraints = column.Constraints;
        if (!constraints.HasFlag(Constraints.AllowDbNull) && columnValue == null)
        {
            throw new ConstraintException(Strings.NullableConstraintViolation(column.Name, Name));
        }

        if (
            constraints.HasFlag(Constraints.Unique) && _tracker.HasChange(column, columnValue, txId)
        )
        {
            throw new ConstraintException(
                Strings.UniqueConstraintViolation(Name, column.Name, columnValue)
            );
        }
    }

    private ValueBuffer GetRowByPK(object primaryKey)
    {
        return _tracker
            .GetChanges()
            .First(change => change.Buffer.GetPrimaryKey() == primaryKey)
            .Buffer;
    }

    private ValueBuffer ResolveColumnValues(ValueBuffer incoming, long txId, bool isRecursiveInsert)
    {
        ValueBuffer result = new();
        foreach (KeyValuePair<ColumnInfo, object> kvp in incoming)
        {
            ColumnInfo column = kvp.Key;
            object columnValue = kvp.Value;

            CheckConstraints(column, columnValue, txId);

            if (column.Reference == null || columnValue == DBNull.Value)
            {
                result[column] = columnValue;
                continue;
            }

            Table dependency = (Table)Relations[column];
            EntityNode depNode = dependency.Node;
            if (column.DataType != depNode.DataType)
            {
                if (!dependency._tracker.HasChange(columnValue, txId))
                {
                    throw new RowNotFoundException(
                        Strings.RowNotFound(dependency.Name, columnValue),
                        dependency.Node.DataType,
                        columnValue
                    );
                }
            }
            else
            {
                ValueBuffer fkBuffer = depNode.Binding.ConvertToValueBuffer(columnValue);
                if (isRecursiveInsert)
                {
                    dependency.Insert(fkBuffer, txId);
                }
                columnValue = fkBuffer.GetPrimaryKey();
            }

            result[column] = columnValue;
        }

        return result;
    }

    private object SelectByBuffer(ValueBuffer buffer)
    {
        ValueBuffer tempBuffer = new();
        foreach (KeyValuePair<ColumnInfo, object> kvp in buffer)
        {
            ColumnInfo column = kvp.Key;
            object columnValue = kvp.Value;

            Type? reference = column.Reference;
            if (reference == null || reference != column.DataType || columnValue == DBNull.Value)
            {
                tempBuffer[column] = columnValue;
                continue;
            }

            Table depTable = (Table)Relations[column];
            ValueBuffer depBuffer = depTable.GetRowByPK(columnValue);
            object depObj = depTable.SelectByBuffer(depBuffer);

            tempBuffer[column] = depObj;
        }

        return Node.Binding.MaterializeEntity(tempBuffer);
    }
}
