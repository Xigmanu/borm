using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

internal sealed class EntityRepository<T> : IEntityRepository<T>
    where T : class
{
    private readonly TableGraph _graph;
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
    }

    public void Delete(T entity)
    {
        using InternalTransaction transaction = new(_graph);
        transaction.Execute(_table, CreateDeleteClosure(entity));
    }

    public void Delete(T entity, Transaction transaction)
    {
        transaction.Execute(_table, CreateDeleteClosure(entity));
    }

    public ValueTask DeleteAsync(T entity)
    {
        return InternalExecuteAsync(entity, Delete);
    }

    public void Insert(T entity)
    {
        using InternalTransaction transaction = new(_graph);
        transaction.Execute(_table, CreateInsertClosure(entity));
    }

    public void Insert(T entity, Transaction transaction)
    {
        transaction.Execute(_table, CreateInsertClosure(entity));
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
        using InternalTransaction transaction = new(_graph);
        transaction.Execute(_table, CreateUpdateClosure(entity));
    }

    public void Update(T entity, Transaction transaction)
    {
        transaction.Execute(_table, CreateUpdateClosure(entity));
    }

    public ValueTask UpdateAsync(T entity)
    {
        return InternalExecuteAsync(entity, Update);
    }

    private Action<long> CreateDeleteClosure(object entity)
    {
        return (txId) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            EntityMetadata metadata = _table.EntityMetadata;
            ValueBuffer buffer = metadata.Binding.ToValueBuffer(entity);
            ValueBuffer preProcessed = _preProcessor.ResolveForeignKeys(
                buffer,
                txId,
                out List<ResolvedForeignKey> _
            );

            _table.Delete(preProcessed, txId);
        };
    }

    private Action<long> CreateInsertClosure(object entity)
    {
        return (txId) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            EntityMetadata metadata = _table.EntityMetadata;
            metadata.Validator?.Invoke(entity);
            ValueBuffer buffer = metadata.Binding.ToValueBuffer(entity);

            InsertRecursively(_table, buffer, txId);
        };
    }

    private Action<long> CreateUpdateClosure(object entity)
    {
        return (txId) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            EntityMetadata metadata = _table.EntityMetadata;
            metadata.Validator?.Invoke(entity);

            ValueBuffer buffer = metadata.Binding.ToValueBuffer(entity);

            ValueBuffer preProcessed = _preProcessor.ResolveForeignKeys(
                buffer,
                txId,
                out List<ResolvedForeignKey> resolvedKeys
            );
            foreach (ResolvedForeignKey resolvedKey in resolvedKeys)
            {
                Table parent = resolvedKey.Table;

                if (resolvedKey.IsComplexRecord)
                {
                    if (!parent.Tracker.TryGetChange(resolvedKey.Value, txId, out _))
                    {
                        throw new RowNotFoundException(
                            Strings.RowNotFound(parent.Name, resolvedKey.Value)
                        );
                    }
                }
                else if (!parent.Tracker.TryGetChange(resolvedKey.Value, txId, out _))
                {
                    throw new RowNotFoundException(
                        Strings.RowNotFound(parent.Name, resolvedKey.Value)
                    );
                }
            }

            _table.Update(preProcessed, txId);
        };
    }

    private void InsertRecursively(Table table, ValueBuffer buffer, long txId)
    {
        ValueBuffer preProcessed = _preProcessor.ResolveForeignKeys(
            buffer,
            txId,
            out List<ResolvedForeignKey> resolvedKeys
        );
        foreach (ResolvedForeignKey resolvedKey in resolvedKeys)
        {
            if (resolvedKey.ChangeExists)
            {
                continue;
            }

            Table parent = resolvedKey.Table;
            EntityMetadata metadata = parent.EntityMetadata;

            if (resolvedKey.IsComplexRecord)
            {
                object rawValue = resolvedKey.RawValue;
                metadata.Validator?.Invoke(rawValue);

                ValueBuffer parentBuffer = metadata.Binding.ToValueBuffer(rawValue);
                InsertRecursively(parent, parentBuffer, txId);
            }
            else
            {
                throw new RowNotFoundException(
                    Strings.RowNotFound(parent.Name, primaryKey: resolvedKey.Value)
                );
            }
        }

        table.Insert(preProcessed, txId);
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
