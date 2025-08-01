﻿using System.Data;
using System.Diagnostics;
using Borm.Data;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Reflection;

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
            throw new InvalidOperationException(Strings.DataContextNotInitialized());
        }

        Type entityType = typeof(T);
        EntityNode node =
            _nodeGraph[entityType]
            ?? throw new ArgumentException(Strings.MissingTableForEntity(entityType.FullName!));
        NodeDataTable? table = _dataSet.Tables[node.Name] as NodeDataTable;
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
        new EntityGraphDataSetMapper(_nodeGraph).LoadMapping(_dataSet);

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

        public DataTable[] Tables => [.. _context._dataSet.Tables.Cast<NodeDataTable>()];
    }
}
