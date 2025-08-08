using System.Data;
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
    private readonly List<Table> _tables;
    private EntityNodeGraph? _nodeGraph;

    public DataContext(BormConfiguration configuration)
    {
        _configuration = configuration;
        _tables = [];
        _nodeGraph = null;
    }

    public event EventHandler? Initialized;

    public Transaction BeginTransaction()
    {
        throw new NotImplementedException();
    }

    public IEntityRepository<T> GetRepository<T>()
        where T : class
    {
        if (_nodeGraph == null)
        {
            throw new InvalidOperationException(Strings.DataContextNotInitialized());
        }

        Type entityType = typeof(T);
        EntityNode node =
            _nodeGraph[entityType]
            ?? throw new ArgumentException(Strings.MissingTableForEntity(entityType.FullName!));
        Table? table = _dataSet.Tables[node.Name] as Table;
        Debug.Assert(table != null);

        return new EntityRepository<T>(table, _nodeGraph);
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

        _nodeGraph = EntityNodeGraphFactory.Create(entityNodes);
        List<Table> tables = new TableCreator(_nodeGraph).MakeTables();
        _tables.AddRange(tables);

        // For now EndInit is called to simply finish the table initialization
        // Later though: TODO check and load data from the database. EndInit is to be called afterwards in order not to screw up the change tracking

        BormDataAdapter adapter = new(
            _configuration.CommandExecutor,
            _nodeGraph,
            _configuration.SqlStatementFactory
        );
        adapter.CreateTables(_dataSet);

        OnInitialized();
    }

    public void RejectChanges()
    {
        _dataSet.RejectChanges();
    }

    public void SaveChanges()
    {
        if (_nodeGraph == null)
        {
            throw new InvalidOperationException(Strings.DataContextNotInitialized());
        }
        if (_dataSet.GetChanges() == null)
        {
            return;
        }

        BormDataAdapter adapter = new(
            _configuration.CommandExecutor,
            _nodeGraph,
            _configuration.SqlStatementFactory
        );

        adapter.Update(_dataSet);
        _dataSet.AcceptChanges();
    }

    public async Task SaveChangesAsync()
    {
        if (_nodeGraph == null)
        {
            throw new InvalidOperationException(Strings.DataContextNotInitialized());
        }
        if (!_dataSet.HasChanges())
        {
            return;
        }

        BormDataAdapter adapter = new(
            _configuration.CommandExecutor,
            _nodeGraph,
            _configuration.SqlStatementFactory
        );

        await adapter.UpdateAsync(_dataSet);
        _dataSet.AcceptChanges();
    }

    private void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    internal sealed class DataContextDebugView
    {
        private readonly DataContext _context;

        public DataContextDebugView(DataContext context)
        {
            _context = context;
        }

        public DataTable[] Tables => [.. _context._dataSet.Tables.Cast<Table>()];
    }
}
