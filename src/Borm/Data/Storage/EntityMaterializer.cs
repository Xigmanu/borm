using System.Diagnostics;
using Borm.Data.Storage.Tracking;
using Borm.Model.Metadata;

namespace Borm.Data.Storage;

internal sealed class EntityMaterializer
{
    private readonly TableGraph _graph;

    public EntityMaterializer(TableGraph graph)
    {
        _graph = graph;
    }

    public object Materialize(IValueBuffer buffer, Table table)
    {
        ValueBuffer tempBuffer = new();

        foreach ((IColumnMetadata column, object columnValue) in buffer)
        {
            bool isSimpleValue = IsColumnValueSimple(column, columnValue);
            tempBuffer[column] = isSimpleValue
                ? columnValue
                : MaterializeParent(_graph[column.Reference!]!, columnValue);
        }

        return table.Metadata.Conversion.MaterializeEntity(tempBuffer);
    }

    [DebuggerStepThrough]
    private static bool IsColumnValueSimple(IColumnMetadata column, object columnValue)
    {
        return column.Reference == null
            || column.Reference != column.DataType.UnderlyingType
            || columnValue.Equals(DBNull.Value);
    }

    private object MaterializeParent(Table parent, object columnValue)
    {
        Debug.Assert(parent is not null);
        // The initial TX ID is used to ensure that I only read committed changes
        bool changeExists = parent.Tracker.TryGetChange(
            columnValue,
            Transaction.InitId,
            out IChange? change
        );
        if (changeExists)
        {
            return Materialize(change!.Record, parent);
        }

        return DBNull.Value;
    }
}
