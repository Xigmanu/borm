using System.Data;
using Borm.Schema.Metadata;

namespace Borm.Data;

internal sealed class NodeDataTableRepository<T> : IEntityRepository<T>
    where T : class
{
    private readonly EntityNodeGraph _nodeGraph;
    private readonly NodeDataTable _table;

    public NodeDataTableRepository(NodeDataTable table, EntityNodeGraph nodeGraph)
    {
        _table = table;
        _nodeGraph = nodeGraph;
    }

    public bool Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        EntityNode node = _table.Node;
        ValueBuffer buffer = node.Binding.ConvertToValueBuffer(entity);
        ColumnInfo primaryKey = node.GetPrimaryKey();

        object primaryKeyValue = buffer[primaryKey];
        DataRow? row = _table.Rows.Find(primaryKeyValue);
        if (row == null)
        {
            return false;
        }

        row.Delete();
        _table.EntityCache.Remove(entity);
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
        _ = InsertRecursively(_table, entity);
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
        List<T> entities = [.. _table.EntityCache.Values.Cast<T>()];
        if (entities.Count != 0)
        {
            return entities;
        }

        foreach (DataRow row in _table.Rows)
        {
            if (row.RowState != DataRowState.Deleted)
            {
                T entity = (T)ReadRowRecursively(_table.Node, row, out object primaryKeyValue);
                entities.Add(entity);

                _table.EntityCache.Add(primaryKeyValue, entity);
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

        EntityNode node = _table.Node;
        ValueBuffer buffer = node.Binding.ConvertToValueBuffer(entity);
        ColumnInfo primaryKey = node.GetPrimaryKey();

        object primaryKeyValue = buffer[primaryKey];
        DataRow? row = _table.Rows.Find(primaryKeyValue);
        if (row == null)
        {
            return false;
        }

        foreach (KeyValuePair<ColumnInfo, object> entryPair in buffer)
        {
            ColumnInfo column = entryPair.Key;
            object newValue = entryPair.Value;

            if (column.Reference == null)
            {
                row[column.Name] = newValue;
                continue;
            }

            EntityNode? parentNode = _nodeGraph[column.DataType];
            if (parentNode == null)
            {
                row[column.Name] = entryPair.Value;
                continue;
            }

            ColumnInfo parentPrimaryKey = parentNode.GetPrimaryKey();
            ValueBuffer parentBuffer = parentNode.Binding.ConvertToValueBuffer(entryPair.Value);
            row[column.Name] = parentBuffer[parentPrimaryKey];
        }

        _table.EntityCache.Update(primaryKeyValue, entity);
        return true;
    }

    public bool Update(T entity, Transaction transaction)
    {
        return transaction.Execute(
            _table.TableName,
            (table) => new NodeDataTableRepository<T>(table, _nodeGraph).Update(entity)
        );
    }

    private object InsertRecursively(NodeDataTable table, object entity)
    {
        EntityNode node = table.Node;
        ValueBuffer buffer = node.Binding.ConvertToValueBuffer(entity);
        ColumnInfo primaryKey = node.GetPrimaryKey();

        object primaryKeyValue = buffer[primaryKey];
        if (table.Rows.Contains(primaryKeyValue))
        {
            return primaryKeyValue;
        }

        IEnumerable<ColumnInfo> foreignKeys = node.Columns.Where(column =>
            column.Reference != null
        );
        foreach (ColumnInfo foreignKey in foreignKeys)
        {
            EntityNode? parentNode = _nodeGraph[foreignKey.DataType];
            if (parentNode == null)
            {
                continue;
            }

            DataRelation parentRelation =
                _table.GetParentRelation(parentNode)
                ?? throw new InvalidOperationException(
                    $"No data relation exists even though two nodes have a relation. Child: {table.Node}, Parent: {parentNode}"
                );

            object parentEntity = buffer[foreignKey];
            buffer[foreignKey] = InsertRecursively(
                (NodeDataTable)parentRelation.ParentTable,
                parentEntity
            );
        }

        DataRow newRow = table.NewRow();
        buffer.LoadIntoRow(newRow);
        _table.Rows.Add(newRow);

        return primaryKeyValue;
    }

    private object ReadRowRecursively(EntityNode node, DataRow row, out object primaryKeyValue)
    {
        ValueBuffer buffer = ValueBuffer.FromDataRow(node, row);
        primaryKeyValue = buffer[node.GetPrimaryKey()];

        IEnumerable<ColumnInfo> foreignKeys = node.Columns.Where(column =>
            column.Reference != null
        );
        foreach (ColumnInfo foreignKey in foreignKeys)
        {
            EntityNode? parentNode = _nodeGraph[foreignKey.DataType];
            if (parentNode == null)
            {
                continue;
            }

            DataRelation parentRelation =
                _table.GetParentRelation(parentNode)
                ?? throw new InvalidOperationException(
                    $"No data relation exists even though two nodes have a relation. Child: {node}, Parent: {parentNode}"
                );

            DataRow parentRow = row.GetParentRow(parentRelation)!;
            buffer[foreignKey] = ReadRowRecursively(parentNode, parentRow, out _);
        }

        return node.Binding.MaterializeEntity(buffer);
    }
}
