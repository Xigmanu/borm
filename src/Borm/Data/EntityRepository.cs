using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

internal sealed class EntityRepository<T> : IEntityRepository<T>
    where T : class
{
    private readonly TableGraph _graph;
    private readonly ReferentialIntegrityHelper _integrityHelper;
    private readonly EntityMaterializer _materializer;
    private readonly BufferPreProcessor _preProcessor;
    private readonly SemaphoreSlim _semaphore;
    private readonly Table _table;

    public EntityRepository(Table table, TableGraph graph)
    {
        _preProcessor = new(graph);
        _graph = graph;
        _semaphore = new(1, 1);
        _table = table;
        _materializer = new(graph);
        _integrityHelper = new(graph);
    }

    public void Delete(T entity)
    {
        using Transaction transaction = new(_graph);
        transaction.Execute(CreateDeleteClosure(entity));
    }

    public void Delete(T entity, Transaction transaction)
    {
        transaction.Execute(CreateDeleteClosure(entity));
    }

    public ValueTask DeleteAsync(T entity)
    {
        return InternalExecuteAsync(entity, Delete);
    }

    public void Insert(T entity)
    {
        using Transaction transaction = new(_graph);
        transaction.Execute(CreateInsertClosure(entity));
    }

    public void Insert(T entity, Transaction transaction)
    {
        transaction.Execute(CreateInsertClosure(entity));
    }

    public ValueTask InsertAsync(T entity)
    {
        return InternalExecuteAsync(entity, Insert);
    }

    public IEnumerable<T> Select()
    {
        return _table
            .Tracker.Changes.Select(change => _materializer.Materialize(change.Buffer, _table))
            .Cast<T>();
    }

    public IEnumerable<R> Select<R>(Func<T, R> selector)
    {
        return Select().Select(selector);
    }

    public Task<IEnumerable<T>> SelectAsync()
    {
        return Task.FromResult(Select());
    }

    public Task<IEnumerable<R>> SelectAsync<R>(Func<T, R> selector)
    {
        return Task.FromResult(Select(selector));
    }

    public void Update(T entity)
    {
        using Transaction transaction = new(_graph);
        transaction.Execute(CreateUpdateClosure(entity));
    }

    public void Update(T entity, Transaction transaction)
    {
        transaction.Execute(CreateUpdateClosure(entity));
    }

    public ValueTask UpdateAsync(T entity)
    {
        return InternalExecuteAsync(entity, Update);
    }

    private static RecordNotFoundException NewRecordNotFoundException(
        Table table,
        object primaryKey
    )
    {
        return new RecordNotFoundException(Strings.RowNotFound(table.Name, primaryKey));
    }

    private static void ValidateForeignKey(long txId, ResolvedForeignKey resolvedKey)
    {
        Table parent = resolvedKey.Parent;

        if (resolvedKey.IsComplexRecord)
        {
            if (!parent.Tracker.TryGetChange(resolvedKey.PrimaryKey, txId, out _))
            {
                throw NewRecordNotFoundException(parent, resolvedKey.PrimaryKey);
            }
        }
        else if (!parent.Tracker.TryGetChange(resolvedKey.PrimaryKey, txId, out _))
        {
            throw NewRecordNotFoundException(parent, resolvedKey.PrimaryKey);
        }
    }

    private Action<long, HashSet<Table>> CreateDeleteClosure(object entity)
    {
        return (txId, affectedTables) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            EntityMetadata metadata = _table.Metadata;
            ValueBuffer buffer = metadata.Binding.ToValueBuffer(entity);
            _ = _preProcessor.ResolveForeignKeys(buffer, txId, out ValueBuffer preProcessed);

            _table.Delete(preProcessed, txId);
            affectedTables.Add(_table);

            HashSet<Table> affectedChildren = _integrityHelper.ApplyDeleteRules(
                _table,
                preProcessed.PrimaryKey,
                txId
            );
            affectedTables.UnionWith(affectedChildren);
        };
    }

    private Action<long, HashSet<Table>> CreateInsertClosure(object entity)
    {
        return (txId, affectedTables) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            EntityMetadata metadata = _table.Metadata;
            metadata.Validator?.Invoke(entity);
            ValueBuffer buffer = metadata.Binding.ToValueBuffer(entity);

            InsertRecursively(_table, buffer, txId, affectedTables);
        };
    }

    private Action<long, HashSet<Table>> CreateUpdateClosure(object entity)
    {
        return (txId, affectedTables) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            EntityMetadata metadata = _table.Metadata;
            metadata.Validator?.Invoke(entity);

            ValueBuffer buffer = metadata.Binding.ToValueBuffer(entity);

            List<ResolvedForeignKey> resolvedKeys = _preProcessor.ResolveForeignKeys(
                buffer,
                txId,
                out ValueBuffer preProcessed
            );
            foreach (ResolvedForeignKey resolvedKey in resolvedKeys)
            {
                ValidateForeignKey(txId, resolvedKey);
            }

            _table.Update(preProcessed, txId);
            affectedTables.Add(_table);
        };
    }

    private void InsertRecursively(
        Table table,
        ValueBuffer buffer,
        long txId,
        HashSet<Table> affectedTables
    )
    {
        List<ResolvedForeignKey> resolvedKeys = _preProcessor.ResolveForeignKeys(
            buffer,
            txId,
            out ValueBuffer preProcessed
        );
        foreach (ResolvedForeignKey resolvedKey in resolvedKeys)
        {
            if (resolvedKey.ChangeExists)
            {
                continue;
            }

            Table parent = resolvedKey.Parent;
            EntityMetadata metadata = parent.Metadata;

            if (resolvedKey.IsComplexRecord)
            {
                object rawValue = resolvedKey.RawValue;
                metadata.Validator?.Invoke(rawValue);

                ValueBuffer parentBuffer = metadata.Binding.ToValueBuffer(rawValue);
                InsertRecursively(parent, parentBuffer, txId, affectedTables);
            }
            else
            {
                throw new RecordNotFoundException(
                    Strings.RowNotFound(parent.Name, primaryKey: resolvedKey.PrimaryKey)
                );
            }
        }

        table.Insert(preProcessed, txId);
        affectedTables.Add(table);
    }

    private async ValueTask InternalExecuteAsync(T entity, Action<T> operation)
    {
        await _semaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
        try
        {
            operation(entity);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
