using Borm.Data.Storage;

namespace Borm.Data.Internal;

internal sealed class EntityRepository<T> : IEntityRepository<T>
    where T : class
{
    private readonly TableGraph _graph;
    private readonly EntityMaterializer _materializer;
    private readonly TransactionOperationFactory _operationFactory;
    private readonly ITable _table;

    public EntityRepository(ITable table, TableGraph graph)
    {
        _graph = graph;
        _table = table;
        _materializer = new EntityMaterializer(graph);
        _operationFactory = new TransactionOperationFactory(
            new RecordPreProcessor(_graph),
            new DeleteRuleRunner(_graph)
        );
    }

    public void Delete(T entity)
    {
        using Transaction transaction = new(_graph);
        Delete(entity, transaction);
    }

    public void Delete(T entity, Transaction transaction)
    {
        TransactionOperation operation = _operationFactory.Create(
            entity,
            _table,
            OperationKind.Delete
        );
        transaction.Execute(operation);
    }

    public void Insert(T entity)
    {
        using Transaction transaction = new(_graph);
        Insert(entity, transaction);
    }

    public void Insert(T entity, Transaction transaction)
    {
        TransactionOperation operation = _operationFactory.Create(
            entity,
            _table,
            OperationKind.Insert
        );
        transaction.Execute(operation);
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
        Update(entity, transaction);
    }

    public void Update(T entity, Transaction transaction)
    {
        TransactionOperation operation = _operationFactory.Create(
            entity,
            _table,
            OperationKind.Update
        );
        transaction.Execute(operation);
    }
}