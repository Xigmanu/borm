using Borm.Data.Storage;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data.Internal;

internal sealed class EntityRepository<T> : IEntityRepository<T>
    where T : class
{
    private readonly TableGraph _graph;
    private readonly ReferentialIntegrityHelper _integrityHelper;
    private readonly EntityMaterializer _materializer;
    private readonly BufferPreProcessor _preProcessor;
    private readonly ITable _table;

    public EntityRepository(ITable table, TableGraph graph)
    {
        _preProcessor = new BufferPreProcessor(graph);
        _graph = graph;
        _table = table;
        _materializer = new EntityMaterializer(graph);
        _integrityHelper = new ReferentialIntegrityHelper(graph);
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

    public void Insert(T entity)
    {
        using Transaction transaction = new(_graph);
        transaction.Execute(CreateInsertClosure(entity));
    }

    public void Insert(T entity, Transaction transaction)
    {
        transaction.Execute(CreateInsertClosure(entity));
    }

    public IEnumerable<T> Select()
    {
        return _table
            .Tracker.Changes.Select(change => _materializer.Materialize(change.Record, _table))
            .Cast<T>();
    }

    public IEnumerable<TR> Select<TR>(Func<T, TR> selector)
    {
        return Select().Select(selector);
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

    private static RecordNotFoundException NewRecordNotFoundException(
        ITable table,
        object primaryKey
    )
    {
        return new RecordNotFoundException(Strings.RowNotFound(table.Name, primaryKey));
    }

    private static void ValidateForeignKey(long txId, ResolvedForeignKey resolvedKey)
    {
        ITable parent = resolvedKey.Parent;

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

    private Action<long, HashSet<ITable>> CreateDeleteClosure(object entity)
    {
        return (txId, affectedTables) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            IEntityMetadata metadata = _table.Metadata;
            IValueBuffer buffer = metadata.Conversion.ToValueBuffer(entity);
            _ = _preProcessor.ResolveForeignKeys(buffer, txId, out IValueBuffer preProcessed);

            _table.Delete(preProcessed, txId);
            affectedTables.Add(_table);

            HashSet<ITable> affectedChildren = _integrityHelper.ApplyDeleteRules(
                _table,
                preProcessed.PrimaryKey,
                txId
            );
            affectedTables.UnionWith(affectedChildren);
        };
    }

    private Action<long, HashSet<ITable>> CreateInsertClosure(object entity)
    {
        return (txId, affectedTables) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            IEntityMetadata metadata = _table.Metadata;
            metadata.Validate(entity);
            IValueBuffer buffer = metadata.Conversion.ToValueBuffer(entity);

            InsertRecursively(_table, buffer, txId, affectedTables);
        };
    }

    private Action<long, HashSet<ITable>> CreateUpdateClosure(object entity)
    {
        return (txId, affectedTables) =>
        {
            ArgumentNullException.ThrowIfNull(entity);

            IEntityMetadata metadata = _table.Metadata;
            metadata.Validate(entity);

            IValueBuffer buffer = metadata.Conversion.ToValueBuffer(entity);

            List<ResolvedForeignKey> resolvedKeys = _preProcessor.ResolveForeignKeys(
                buffer,
                txId,
                out IValueBuffer preProcessed
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
        ITable table,
        IValueBuffer buffer,
        long txId,
        HashSet<ITable> affectedTables
    )
    {
        List<ResolvedForeignKey> resolvedKeys = _preProcessor.ResolveForeignKeys(
            buffer,
            txId,
            out IValueBuffer preProcessed
        );
        foreach ((ITable parent, object primaryKey, object rawValue, bool isComplexRecord, bool changeExists) in
                 resolvedKeys)
        {
            if (changeExists)
            {
                continue;
            }

            IEntityMetadata metadata = parent.Metadata;

            if (isComplexRecord)
            {
                metadata.Validate(rawValue);

                IValueBuffer parentBuffer = metadata.Conversion.ToValueBuffer(rawValue);
                InsertRecursively(parent, parentBuffer, txId, affectedTables);
            }
            else
            {
                throw new RecordNotFoundException(
                    Strings.RowNotFound(parent.Name, primaryKey)
                );
            }
        }

        table.Insert(preProcessed, txId);
        affectedTables.Add(table);
    }
}