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

    public object Materialize(ValueBuffer buffer, Table table)
    {
        ValueBuffer tempBuffer = new();

        foreach ((ColumnMetadata column, object columnValue) in buffer)
        {
            bool isSimpleValue =
                column.Reference == null
                || column.Reference != column.DataType
                || columnValue.Equals(DBNull.Value);

            tempBuffer[column] = isSimpleValue
                ? columnValue
                : MaterializeParent(_graph[column.Reference!]!, columnValue);
        }

        return table.EntityMetadata.Binding.MaterializeEntity(tempBuffer);
    }

    private object MaterializeParent(Table parent, object columnValue)
    {
        Debug.Assert(parent is not null);
        // The initial TX ID is used to ensure that I only read committed changes
        bool changeExists = parent.Tracker.TryGetChange(
            columnValue,
            InternalTransaction.InitId,
            out Change? change
        );
        if (changeExists)
        {
            return Materialize(change!.Buffer, parent);
        }

        return DBNull.Value;
    }
}
