using System.Data;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

/*
 * TODO
 *
 * 1. Implement equality comparison for HashSet
 */
internal sealed class Table
{
    private readonly ObjectCache _entityCache = new();
    private readonly EntityNode _node;
    private readonly ChangeTracker _tracker = new();

    public Table(string name, EntityNode node)
    {
        _node = node;
        Name = name;
    }

    public string Name { get; }
    internal TableSet Dependencies { get; } = new();
    internal ObjectCache EntityCache => _entityCache; // TODO rework this maybe
    internal EntityNode Node => _node;

    public void AcceptPendingChanges(long txId)
    {
        _tracker.AcceptPendingChanges(txId);
    }

    public void Delete(object entity, long txId)
    {
        ValueBuffer buffer = _node.Binding.ConvertToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();
        if (!_tracker.HasRow(primaryKey))
        {
            throw new RowNotFoundException(
                Strings.RowNotFound(Name, primaryKey),
                Node.DataType,
                primaryKey
            );
        }

        Change change = new(buffer, txId, RowAction.Delete);
        _tracker.PendUpdate(change, txId);
    }

    public void Insert(object entity, long txId)
    {
        ValueBuffer buffer = _node.Binding.ConvertToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();
        if (_tracker.HasRow(primaryKey))
        {
            throw new ConstraintException(Strings.PrimaryKeyConstraintViolation(Name, primaryKey));
        }

        ValueBuffer incoming = ResolveColumnValues(this, buffer, txId, isRecursiveInsert: true);

        Change change = new(incoming, txId, RowAction.Insert);
        _tracker.PendUpdate(change, txId);
    }

    public IEnumerable<object> Select()
    {
        IEnumerable<Change> changes = _tracker.GetChanges();
        return changes.Select(change => SelectByBuffer(change.Buffer));
    }

    public void Update(object entity, long txId)
    {
        ValueBuffer buffer = _node.Binding.ConvertToValueBuffer(entity);
        object primaryKey = buffer.GetPrimaryKey();
        if (!_tracker.HasRow(primaryKey))
        {
            throw new RowNotFoundException(
                Strings.RowNotFound(Name, primaryKey),
                Node.DataType,
                primaryKey
            );
        }

        ColumnInfoCollection columns = _node.Columns;
        ValueBuffer incoming = ResolveColumnValues(this, buffer, txId, isRecursiveInsert: false);

        Change change = new(incoming, txId, RowAction.Update);
        _tracker.PendUpdate(change, txId);
    }
    private static void CheckConstraints(ColumnInfo column, object columnValue)
    {
        switch (column.Constraints)
        {
            case Constraints.Unique:

                break;
            case Constraints.AllowDbNull:
                if (columnValue == null)
                {
                    throw new Exception();
                }
                break;
        }
    }

    private static ValueBuffer ResolveColumnValues(
        Table table,
        ValueBuffer incoming,
        long txId,
        bool isRecursiveInsert
    )
    {
        ValueBuffer result = new();
        foreach (KeyValuePair<ColumnInfo, object> kvp in incoming)
        {
            ColumnInfo column = kvp.Key;
            object columnValue = kvp.Value;

            CheckConstraints(column, columnValue);

            if (column.Reference == null || columnValue == null)
            {
                result[column] = columnValue;
                continue;
            }

            Table dependency = table.Dependencies.First(dep =>
                dep.Node.DataType == column.Reference
            );
            EntityNode depNode = dependency.Node;
            if (column.DataType != depNode.DataType)
            {
                if (!dependency._tracker.HasRow(columnValue))
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

    private ValueBuffer GetRowByPK(object primaryKey)
    {
        return _tracker
            .GetChanges()
            .First(change => change.Buffer.GetPrimaryKey() == primaryKey)
            .Buffer;
    }

    private object SelectByBuffer(ValueBuffer buffer)
    {
        ValueBuffer tempBuffer = new();
        foreach (KeyValuePair<ColumnInfo, object?> kvp in buffer)
        {
            ColumnInfo column = kvp.Key;
            object? columnValue = kvp.Value;

            Type? reference = column.Reference;
            if (reference == null || reference != column.DataType || columnValue == null)
            {
                tempBuffer[column] = columnValue;
                continue;
            }

            Table depTable = Dependencies.First(table => table.Node.DataType == reference);
            ValueBuffer depBuffer = depTable.GetRowByPK(columnValue);
            object depObj = depTable.SelectByBuffer(depBuffer);

            tempBuffer[column] = depObj;
        }

        return Node.Binding.MaterializeEntity(tempBuffer);
    }
}
