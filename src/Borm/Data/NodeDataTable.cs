using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Schema;

namespace Borm.Data;

[DebuggerTypeProxy(typeof(NodeDataTableDebugView))]
internal sealed class NodeDataTable : DataTable
{
    private readonly EntityObjectCache _entityCache;
    private readonly TableNode _node;

    public NodeDataTable()
        : base()
    {
        _node = null!;
        _entityCache = null!;
    }

    public NodeDataTable(string tableName, TableNode node)
        : base(tableName)
    {
        _node = node;
        _entityCache = new();
    }

    private NodeDataTable(NodeDataTable original)
        : this(original.TableName, original._node) { }

    internal EntityObjectCache EntityCache => _entityCache;
    internal TableNode Node => _node;

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

    public DataRelation? GetParentRelation(TableNode node)
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

    public ColumnInfo[] GetTableOrderedNodeColumns()
    {
        List<ColumnInfo> result = [];
        foreach (DataColumn tableColumn in Columns)
        {
            ColumnInfo nodeColumn = _node.Columns.First(column =>
                column.Name == tableColumn.ColumnName
            );
            result.Add(nodeColumn);
        }
        return [.. result];
    }

    public bool IsEntityTypeValid(
        object entityObj,
        [NotNullWhen(false)] out TypeMismatchException? exception
    )
    {
        exception = null;
        Type entityType = entityObj.GetType();
        if (!entityType.Equals(_node.DataType))
        {
            exception = new TypeMismatchException(
                "Provided entity type is not equal to node's data type",
                _node.DataType,
                entityType
            );
            return false;
        }
        return true;
    }

    [ExcludeFromCodeCoverage]
    internal sealed class NodeDataTableDebugView
    {
        private readonly NodeDataTable _nodeDataTable;

        public NodeDataTableDebugView(NodeDataTable nodeDataTable)
        {
            _nodeDataTable = nodeDataTable;
        }

        public DataColumn[] Columns => [.. _nodeDataTable.Columns.Cast<DataColumn>()];
        public string Name => _nodeDataTable.TableName;
        public TableNode Node => _nodeDataTable._node;
        public DataRow[] Rows => [.. _nodeDataTable.Rows.Cast<DataRow>()];
    }
}
