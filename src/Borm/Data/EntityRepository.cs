using System.Diagnostics;
using System.Runtime.CompilerServices;
using Borm.Data.Storage;
using Borm.Model;
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
            _ = _preProcessor.ResolveForeignKeys(buffer, txId, out ValueBuffer preProcessed);

            _table.Delete(preProcessed, txId);
            ExecuteReferentialIntegrityOperation(txId, preProcessed.PrimaryKey);
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

            List<ResolvedForeignKey> resolvedKeys = _preProcessor.ResolveForeignKeys(
                buffer,
                txId,
                out ValueBuffer preProcessed
            );
            foreach (ResolvedForeignKey resolvedKey in resolvedKeys)
            {
                Table parent = resolvedKey.Parent;

                if (resolvedKey.IsComplexRecord)
                {
                    if (!parent.Tracker.TryGetChange(resolvedKey.PrimaryKey, txId, out _))
                    {
                        throw new RowNotFoundException(
                            Strings.RowNotFound(parent.Name, resolvedKey.PrimaryKey)
                        );
                    }
                }
                else if (!parent.Tracker.TryGetChange(resolvedKey.PrimaryKey, txId, out _))
                {
                    throw new RowNotFoundException(
                        Strings.RowNotFound(parent.Name, resolvedKey.PrimaryKey)
                    );
                }
            }

            _table.Update(preProcessed, txId);
            // ExecuteReferentialIntegrityOperation(txId, preProcessed.PrimaryKey);
        };
    }

    private void ExecuteReferentialIntegrityOperation(
        long txId,
        object parentPrimaryKey,
        [CallerMemberName] string? operation = null
    )
    {
        Debug.Assert(
            operation == nameof(CreateDeleteClosure) || operation == nameof(CreateUpdateClosure),
            $"{nameof(ExecuteReferentialIntegrityOperation)} should only be called during Update/Delete"
        );

        IEnumerable<Table> children = _graph.GetChildren(_table);
        foreach (Table child in children)
        {
            EntityMetadata childMetadata = child.EntityMetadata;
            foreach (ColumnMetadata column in childMetadata.Columns)
            {
                if (column.Reference is null)
                {
                    continue;
                }

                IEnumerable<ValueBuffer> affected = child
                    .Tracker.Changes.Where(change =>
                        Equals(change.Buffer[column], parentPrimaryKey)
                    )
                    .Select(change => change.Buffer.Copy()); // Copy here as a safeguard against mutations via reference
                foreach (ValueBuffer buffer in affected)
                {
                    if (operation == nameof(CreateUpdateClosure))
                    {
                        continue; // FIXME
                        object value = column.OnUpdate switch
                        {
                            ReferentialAction.Cascade => parentPrimaryKey,
                            ReferentialAction.SetNull => DBNull.Value,
                            _ => throw new NotSupportedException(
                                $"Unexpected {nameof(ReferentialAction)}: {column.OnUpdate}"
                            ),
                        };
                        buffer[column] = value;

                        child.Update(buffer, txId);
                    }
                    else
                    {
                        if (column.OnDelete == ReferentialAction.SetNull)
                        {
                            buffer[column] = DBNull.Value;
                            child.Update(buffer, txId);
                        }
                        else if (column.OnDelete == ReferentialAction.Cascade)
                        {
                            child.Delete(buffer, txId);
                        }
                    }
                }
            }
        }
    }

    private void InsertRecursively(Table table, ValueBuffer buffer, long txId)
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
                    Strings.RowNotFound(parent.Name, primaryKey: resolvedKey.PrimaryKey)
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
