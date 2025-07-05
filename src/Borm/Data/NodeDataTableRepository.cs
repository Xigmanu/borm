using System.Data;
using Borm.Schema;

namespace Borm.Data;

internal sealed class NodeDataTableRepository<T> : IEntityRepository<T>
    where T : class
{
    private readonly TableNodeGraph _nodeGraph;
    private readonly NodeDataTable _table;

    public NodeDataTableRepository(NodeDataTable table, TableNodeGraph nodeGraph)
    {
        _table = table;
        _nodeGraph = nodeGraph;
    }

    public bool Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!_table.IsEntityTypeValid(entity, out TypeMismatchException? exception))
        {
            throw exception;
        }

        object primaryKey = _table.Node.GetPrimaryKeyValue(entity);
        DataRow? row = _table.Rows.Find(primaryKey);
        if (row == null)
        {
            return false;
        }
        row.Delete();
        _table.EntityCache.Remove(primaryKey);

        return true;
    }

    public bool Delete(T entity, Transaction transaction)
    {
        return transaction.Execute(
            _table.TableName,
            (table) => new NodeDataTableRepository<T>(table, _nodeGraph).Delete(entity)
        );
    }

    public bool Insert(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!_table.IsEntityTypeValid(entity, out TypeMismatchException? exception))
        {
            throw exception;
        }

        object primaryKey = _table.Node.GetPrimaryKeyValue(entity);
        if (_table.Rows.Find(primaryKey) != null)
        {
            return false;
        }

        ColumnInfo[] orderedColumns = _table.GetTableOrderedNodeColumns();
        DataRow row = _table.NewRow();
        for (int i = 0; i < orderedColumns.Length; i++)
        {
            ColumnInfo columnInfo = orderedColumns[i];
            object columnValue = columnInfo.GetValue(entity) ?? DBNull.Value;

            if (columnInfo.ReferencedEntityType == null)
            {
                row[i] = columnValue;
                continue;
            }

            TableNode? parentNode = _nodeGraph[columnInfo.DataType];
            if (parentNode == null || columnValue.Equals(DBNull.Value))
            {
                row[i] = columnValue;
            }

            NodeDataTable parentTable = (NodeDataTable)_table.DataSet!.Tables[parentNode!.Name]!;
            new NodeDataTableRepository<object>(parentTable, _nodeGraph).Insert(columnValue);
            row[i] = parentNode.GetPrimaryKeyValue(columnValue);
        }

        _table.Rows.Add(row);
        _table.EntityCache.Add(primaryKey, entity);

        return true;
    }

    public bool Insert(T entity, Transaction transaction)
    {
        return transaction.Execute(
            _table.TableName,
            (table) => new NodeDataTableRepository<T>(table, _nodeGraph).Insert(entity)
        );
    }

    public IEnumerable<T> Select()
    {
        List<T> entities = [.. _table.EntityCache.Entities.Cast<T>()];
        if (entities.Count != 0)
        {
            return entities;
        }

        foreach (DataRow row in _table.Rows)
        {
            if (row.RowState != DataRowState.Deleted)
            {
                T entity = (T)ReadRowRecursively(_table, row);
                entities.Add(entity);

                object primaryKey = _table.Node.GetPrimaryKeyValue(entity);
                _table.EntityCache.Add(primaryKey, entity);
            }
        }

        return entities;
    }

    public IEnumerable<R> Select<R>(Func<T, R> selector)
    {
        return Select().Select(selector);
    }

    public bool Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!_table.IsEntityTypeValid(entity, out TypeMismatchException? exception))
        {
            throw exception;
        }

        ColumnInfo[] orderedColumns = _table.GetTableOrderedNodeColumns();
        object primaryKey = _table.Node.GetPrimaryKeyValue(entity);
        DataRow? row = _table.Rows.Find(primaryKey);
        if (row == null)
        {
            return false;
        }

        for (int i = 0; i < orderedColumns.Length; i++)
        {
            ColumnInfo columnInfo = orderedColumns[i];
            if (columnInfo.ReferencedEntityType == null)
            {
                row[i] = columnInfo.GetValue(entity) ?? DBNull.Value;
            }
        }
        _table.EntityCache.Update(primaryKey, entity);

        return true;
    }

    public bool Update(T entity, Transaction transaction)
    {
        return transaction.Execute(
            _table.TableName,
            (table) => new NodeDataTableRepository<T>(table, _nodeGraph).Update(entity)
        );
    }

    private object ReadRowRecursively(NodeDataTable table, DataRow row)
    {
        int columnCount = table.Rows.Count;
        object?[] ctorArgs = new object?[columnCount];
        ColumnInfo[] orderedColumns = _table.GetTableOrderedNodeColumns();
        for (int i = 0; i < orderedColumns.Length; i++)
        {
            ColumnInfo columnInfo = orderedColumns[i];
            object? columnValue = row[columnInfo.Name];
            if (columnValue.GetType().Equals(typeof(DBNull)))
            {
                columnValue = null;
            }

            if (columnInfo.ReferencedEntityType == null)
            {
                ctorArgs[i] = columnValue;
                continue;
            }

            TableNode? parentNode = _nodeGraph[columnInfo.DataType];
            if (parentNode == null)
            {
                ctorArgs[i] = columnValue;
                continue;
            }

            DataRelation parentRelation =
                _table.GetParentRelation(parentNode)
                ?? throw new InvalidOperationException(
                    $"No data relation exists even though two nodes have a relation. Child: {table.Node}, Parent: {parentNode}"
                );

            DataRow parentRow = row.GetParentRow(parentRelation)!;
            object? parentRowValue = ReadRowRecursively((NodeDataTable)parentRow.Table, parentRow);
            ctorArgs[i] = parentRowValue;
        }

        return table.Node.CreateInstance(ctorArgs);
    }
}
