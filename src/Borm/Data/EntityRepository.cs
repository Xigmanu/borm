using System.Data;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

internal sealed class EntityRepository<T> : IEntityRepository<T>
    where T : class
{
    private readonly EntityNodeGraph _nodeGraph;
    private readonly SemaphoreSlim _semaphore;
    private readonly NodeDataTable _table;

    public EntityRepository(NodeDataTable table, EntityNodeGraph nodeGraph)
    {
        _table = table;
        _semaphore = new(1, 1);
        _nodeGraph = nodeGraph;
    }

    public void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        EntityNode node = _table.Node;
        ValueBuffer buffer = node.Binding.ConvertToValueBuffer(entity);
        ColumnInfo primaryKey = node.GetPrimaryKey();

        object primaryKeyValue = buffer[primaryKey];
        DataRow? row =
            _table.Rows.Find(primaryKeyValue)
            ?? throw new RowNotFoundException(
                Strings.RowNotFound(_table.TableName, primaryKeyValue),
                node.DataType,
                primaryKeyValue
            );

        row.Delete();
        _table.EntityCache.Remove(entity);
    }

    public void Delete(T entity, Transaction transaction)
    {
        transaction.Execute(
            _table.TableName,
            (table) => new EntityRepository<T>(table, _nodeGraph).Delete(entity)
        );
    }

    public Task DeleteAsync(T entity)
    {
        return ExecuteInLock(() => Delete(entity));
    }

    public void Insert(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = InsertRecursively(_table, entity);
    }

    public void Insert(T entity, Transaction transaction)
    {
        transaction.Execute(
            _table.TableName,
            (table) => new EntityRepository<T>(table, _nodeGraph).Insert(entity)
        );
    }

    public Task InsertAsync(T entity)
    {
        return ExecuteInLock(() => Insert(entity));
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

    public Task<IEnumerable<T>> SelectAsync()
    {
        return Task.Run(Select);
    }

    public Task<IEnumerable<R>> SelectAsync<R>(Func<T, R> selector)
    {
        return Task.Run(() => Select(selector));
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        EntityNode node = _table.Node;
        node.Validator?.Invoke(entity);

        ValueBuffer buffer = node.Binding.ConvertToValueBuffer(entity);
        ColumnInfo primaryKey = node.GetPrimaryKey();

        object primaryKeyValue = buffer[primaryKey];
        DataRow row =
            _table.Rows.Find(primaryKeyValue)
            ?? throw new RowNotFoundException(
                Strings.RowNotFound(_table.TableName, primaryKeyValue),
                node.DataType,
                primaryKeyValue
            );

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
    }

    public void Update(T entity, Transaction transaction)
    {
        transaction.Execute(
            _table.TableName,
            (table) => new EntityRepository<T>(table, _nodeGraph).Update(entity)
        );
    }

    public Task UpdateAsync(T entity)
    {
        return ExecuteInLock(() => Update(entity));
    }

    private async Task ExecuteInLock(Action action)
    {
        await _semaphore.WaitAsync();
        try
        {
            action();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private object InsertRecursively(NodeDataTable table, object entity)
    {
        EntityNode node = table.Node;
        node.Validator?.Invoke(entity);

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
                    Strings.MissingExpectedDataRelation(parentNode, node)
                );

            object parentEntity = buffer[foreignKey];
            buffer[foreignKey] = InsertRecursively(
                (NodeDataTable)parentRelation.ParentTable,
                parentEntity
            );
        }

        DataRow newRow = table.NewRow();
        buffer.LoadIntoRow(newRow);
        table.Rows.Add(newRow);

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
                    Strings.MissingExpectedDataRelation(parentNode, node)
                );

            DataRow parentRow = row.GetParentRow(parentRelation)!;
            buffer[foreignKey] = ReadRowRecursively(parentNode, parentRow, out _);
        }

        return node.Binding.MaterializeEntity(buffer);
    }
}
