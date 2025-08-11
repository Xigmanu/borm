using System.Diagnostics;
using Borm.Data;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Reflection;

namespace Borm;

public sealed class DataContext
{
    private readonly BormConfiguration _configuration;
    private readonly BormDataAdapter _dataAdapter;
    private readonly TableGraph _tableGraph;

    public DataContext(BormConfiguration configuration)
    {
        _configuration = configuration;
        _tableGraph = new();
        _dataAdapter = new(
            configuration.CommandExecutor,
            _tableGraph,
            configuration.SqlStatementFactory
        );
    }

    public event EventHandler? Initialized;

    public Transaction BeginTransaction()
    {
        return new Transaction(this, _configuration.TransactionWriteOnCommit);
    }

    public IEntityRepository<T> GetRepository<T>()
        where T : class
    {
        if (_tableGraph == null)
        {
            throw new InvalidOperationException(Strings.DataContextNotInitialized());
        }

        Type entityType = typeof(T);
        Table table =
            _tableGraph[entityType]
            ?? throw new ArgumentException(Strings.MissingTableForEntity(entityType.FullName!));
        Debug.Assert(table != null);

        return new EntityRepository<T>(table);
    }

    public void Initialize()
    {
        EntityModel model = _configuration.Model;
        IEnumerable<ReflectedTypeInfo> typeInfos = model.GetReflectedInfo();
        if (!typeInfos.Any())
        {
            return;
        }

        List<EntityNode> entityNodes = new(typeInfos.Count());
        foreach (ReflectedTypeInfo typeInfo in typeInfos)
        {
            EntityNode node = EntityNodeFactory.Create(typeInfo);

            BindingInfo bindingInfo = new(typeInfo.Type, node.Columns);
            node.Binding = bindingInfo.CreateBinding();
            node.Validator = model.GetValidatorFunc(typeInfo.Type);

            entityNodes.Add(node);
        }

        EntityNodeValidator validator = new(entityNodes);
        entityNodes.ForEach(node =>
        {
            if (!validator.IsValid(node, out Exception? exception))
            {
                throw exception;
            }
        });

        IEnumerable<Table> tables = new TableGraphBuilder(entityNodes).BuildAll();
        _tableGraph.AddTableRange(tables);

        BormDataAdapter adapter = new(
            _configuration.CommandExecutor,
            _tableGraph,
            _configuration.SqlStatementFactory
        );
        adapter.CreateTables();

        OnInitialized(); //TODO Replace this with a trigger
    }

    public void SaveChanges()
    {
        _dataAdapter.Update();
    }

    public Task SaveChangesAsync()
    {
        return _dataAdapter.UpdateAsync();
    }

    private void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
    }
}
