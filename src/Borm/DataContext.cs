using System.Data;
using System.Diagnostics;
using System.Reflection;
using Borm.Data;
using Borm.Schema;
using Borm.Schema.Metadata;

namespace Borm;

public sealed class DataContext : IDisposable
{
    private const string DefaultDataStoreName = "borm_data";

    private readonly BormConfiguration _configuration;
    private readonly BormDataSet _dataSet;
    private EntityNodeGraph? _nodeGraph;

    public DataContext(BormConfiguration configuration)
    {
        _configuration = configuration;
        _dataSet = new BormDataSet(DefaultDataStoreName);
        _nodeGraph = null;
    }

    public event EventHandler? Initialized;

    internal BormDataSet DataSet => _dataSet;

    public Transaction BeginTransaction()
    {
        return new Transaction(this, _configuration.TransactionWriteOnCommit);
    }

    public void Dispose()
    {
        _dataSet.Dispose();
    }

    public IEntityRepository<T> GetRepository<T>()
        where T : class
    {
        if (_nodeGraph == null)
        {
            throw new InvalidOperationException("Data context is not initialized");
        }

        Type entityType = typeof(T);
        EntityNode node =
            _nodeGraph[entityType]
            ?? throw new ArgumentException(
                $"No table with data type {entityType.FullName} exists in the data context"
            );
        NodeDataTable? table = _dataSet.Tables[node.Name] as NodeDataTable;
        Debug.Assert(table != null);

        return new NodeDataTableRepository<T>(table, _nodeGraph);
    }

    public void Initialize(Assembly entitySource)
    {
        using IDbConnection connection = _configuration.DbConnectionSupplier();
        try
        {
            IEnumerable<Type> entityTypes = EntityTypeResolver.GetTypes(
                entitySource.GetExportedTypes()
            );
            if (!entityTypes.Any())
            {
                return;
            }

            _nodeGraph = new EntityNodeGraphFactory(entityTypes).Create();
            new EntityGraphDataSetBuilder(_nodeGraph).LoadMapping(_dataSet);

            connection.Open();

            BormDataAdapter dataAdapter = new(
                connection.CreateCommand(),
                _nodeGraph,
                _configuration.SqlStatementFactory
            );
            dataAdapter.CreateTables(_dataSet);

            OnInitialized();
        }
        catch (Exception ex)
        {
            connection.Close();
            throw new InvalidOperationException("Failed to initialize data context", ex);
        }
    }

    public void RejectChanges()
    {
        _dataSet.RejectChanges();
    }

    public void SaveChanges()
    {
        if (_dataSet.GetChanges() == null)
        {
            return;
        }
        if (_nodeGraph == null)
        {
            throw new InvalidOperationException("Data context is not initialized");
        }

        try
        {
            using IDbConnection connection = _configuration.DbConnectionSupplier();
            connection.Open();
            using IDbCommand command = connection.CreateCommand();

            BormDataAdapter dataAdapter = new(
                command,
                _nodeGraph,
                _configuration.SqlStatementFactory
            );
            dataAdapter.Update(_dataSet);
            _dataSet.AcceptChanges();
            connection.Close();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to update the data source", ex);
        }
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

        public DataTable[] Tables => [.. _context._dataSet.Tables.Cast<NodeDataTable>()];
    }
}
