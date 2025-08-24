using System.Diagnostics;
using Borm.Data;
using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Reflection;

namespace Borm;

public sealed class DataContext
{
    private readonly BormConfiguration _configuration;
    private readonly DataAdapter _dataAdapter;
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

        List<EntityMetadata> entityInfos = new(typeInfos.Count());
        foreach (ReflectedTypeInfo typeInfo in typeInfos)
        {
            EntityMetadata entityMetadata = EntityMetadataBuilder.Build(typeInfo);

            EntityMaterializationBinding binding = new(typeInfo.Type, entityMetadata.Columns);
            entityMetadata.Binding = binding.CreateBinding();
            entityMetadata.Validator = model.GetValidatorFunc(typeInfo.Type);

            entityInfos.Add(entityMetadata);
        }

        EntityInfoValidator validator = new(entityInfos);
        entityInfos.ForEach(info =>
        {
            if (!validator.IsValid(info, out Exception? exception))
            {
                throw exception;
            }
        });

        IEnumerable<Table> tables = new TableGraphBuilder(entityInfos).BuildAll();
        _tableGraph.AddTableRange(tables);

        _dataAdapter.CreateTables();

        OnInitialized();
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
