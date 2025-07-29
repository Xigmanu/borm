using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

[DebuggerTypeProxy(typeof(NodeDataTableDebugView))]
internal sealed class NodeDataTable : DataTable
{
    private readonly ChangeTracker _changeTracker = new();
    private readonly ObjectCache _entityCache = new();
    private readonly EntityNode _node;
    private bool _isInitialized = false;

    public NodeDataTable()
        : base()
    {
        _node = null!;
    }

    public NodeDataTable(string tableName, EntityNode node)
        : base(tableName)
    {
        _node = node;
        Initialized += NodeDataTable_Initialized;
    }

    private NodeDataTable(NodeDataTable original)
        : this(original.TableName, original._node) { }

    internal ObjectCache EntityCache => _entityCache;

    internal EntityNode Node => _node;

    public new NodeDataTable Copy()
    {
        NodeDataTable copy = new(this);
        foreach (DataColumn column in Columns)
        {
            DataColumn copyColumn = new(column.ColumnName, column.DataType)
            {
                AllowDBNull = column.AllowDBNull,
            };
            copy.Columns.Add(copyColumn);
        }
        copy.PrimaryKey = [copy.Columns[PrimaryKey[0].ColumnName]!];
        foreach (DataRow row in Rows)
        {
            copy.ImportRow(row);
        }

        return copy;
    }

    public DataRelation? GetParentRelation(EntityNode node)
    {
        string relationName = $"{TableName}_{node.Name}";
        foreach (DataRelation parentRelation in ParentRelations)
        {
            if (parentRelation.RelationName == relationName)
            {
                return parentRelation;
            }
        }
        return null;
    }

    public VersionedDataRow GetRowByPK(object primaryKey)
    {
        return (VersionedDataRow)(
            Rows.Find(primaryKey)
            ?? throw new RowNotFoundException(
                Strings.RowNotFound(TableName, primaryKey),
                _node.DataType,
                primaryKey
            )
        );
    }

    public new VersionedDataRow NewRow()
    {
        return (VersionedDataRow)base.NewRow();
    }

    protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
    {
        return new VersionedDataRow(builder);
    }

    protected override void OnRowChanged(DataRowChangeEventArgs e)
    {
        VersionedDataRow row = (VersionedDataRow)e.Row;
        object primaryKey = row[PrimaryKey[0]];

        if (!_isInitialized)
        {
            _changeTracker.InitInsert(primaryKey, row);
        }
        else
        {
            _changeTracker.Update(primaryKey, row, e.Action);
        }
    }

    protected override void OnRowDeleting(DataRowChangeEventArgs e)
    {
        VersionedDataRow row = (VersionedDataRow)e.Row;
        _changeTracker.Update(row[PrimaryKey[0]], row, DataRowAction.Delete);
    }

    private void NodeDataTable_Initialized(object? sender, EventArgs e)
    {
        _isInitialized = true;
    }

    [ExcludeFromCodeCoverage(Justification = "Debug view class")]
    internal sealed class NodeDataTableDebugView
    {
        private readonly NodeDataTable _nodeDataTable;

        public NodeDataTableDebugView(NodeDataTable nodeDataTable)
        {
            _nodeDataTable = nodeDataTable;
        }

        public DataColumn[] Columns => [.. _nodeDataTable.Columns.Cast<DataColumn>()];
        public string Name => _nodeDataTable.TableName;
        public EntityNode Node => _nodeDataTable._node;
        public DataRow[] Rows => [.. _nodeDataTable.Rows.Cast<DataRow>()];
    }
}
