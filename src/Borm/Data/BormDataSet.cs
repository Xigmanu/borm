using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Borm.Extensions;

namespace Borm.Data;

[DebuggerTypeProxy(typeof(BormDataSetDebugView))]
internal sealed class BormDataSet : DataSet
{
    private readonly Dictionary<DataTable, List<DataRowInfo>> _deletedRowMap;

    public BormDataSet()
        : base()
    {
        _deletedRowMap = [];
    }

    public BormDataSet(string dataSetName)
        : base(dataSetName)
    {
        _deletedRowMap = [];
    }

    private BormDataSet(BormDataSet original)
        : this(original.DataSetName)
    {
        _deletedRowMap = original._deletedRowMap;
    }

    public void AddTable(NodeDataTable nodeDataTable)
    {
        Tables.Add(nodeDataTable);
        nodeDataTable.RowDeleting += NodeDataTable_RowDeleting;
        nodeDataTable.RowDeleted += NodeDataTable_RowDeleted;
    }

    public new BormDataSet Copy()
    {
        BormDataSet dataSetCopy = new(this);
        foreach (DataTable table in Tables)
        {
            NodeDataTable tableCopy = ((NodeDataTable)table).Copy();
            dataSetCopy.Tables.Add(tableCopy);
        }
        foreach (DataRelation relation in Relations)
        {
            DataRelation relationCopy = relation.Copy(dataSetCopy);
            dataSetCopy.Relations.Add(relationCopy);
        }

        return dataSetCopy;
    }

    internal DataRow GetDeletedRowClone(DataTable table, int deletedRowIndex)
    {
        DataTable changes = table.GetChanges()!;
        Debug.Assert(changes != null);

        List<DataRowInfo> rows = _deletedRowMap[table];
        DataRowInfo match = rows.First(info => info.Idx == deletedRowIndex);
        return match.ClonedRow;
    }

    private void NodeDataTable_RowDeleted(object sender, DataRowChangeEventArgs e)
    {
        DataTable senderTable = (DataTable)sender;
        List<DataRowInfo> rows = _deletedRowMap[senderTable];
        DataRowInfo lastRow = rows[^1];
        DataTable changes = senderTable.GetChanges()!;
        lastRow.Idx = changes.Rows.Count - 1;
    }

    private void NodeDataTable_RowDeleting(object sender, DataRowChangeEventArgs e)
    {
        DataTable senderTable = (DataTable)sender;

        DataRow clonedRow = senderTable.NewRow();
        clonedRow.ItemArray = (object?[])e.Row.ItemArray.Clone();

        DataRowInfo info = new(clonedRow);
        if (!_deletedRowMap.TryAdd(senderTable, [info]))
        {
            List<DataRowInfo> rows = _deletedRowMap[senderTable];
            rows.Add(info);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "Debug code")]
    internal sealed class BormDataSetDebugView
    {
        private readonly BormDataSet _dataSet;

        public BormDataSetDebugView(BormDataSet dataSet)
        {
            _dataSet = dataSet;
        }

        public DataRelation[] Relations => [.. _dataSet.Relations.Cast<DataRelation>()];
        public NodeDataTable[] Tables => [.. _dataSet.Tables.Cast<NodeDataTable>()];
    }

    private sealed class DataRowInfo(DataRow clonedRow)
    {
        public DataRow ClonedRow { get; } = clonedRow;

        public int Idx { get; set; }
    }
}
