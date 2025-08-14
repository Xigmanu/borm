using Borm.Model.Metadata;

namespace Borm.Data;

internal sealed class EntityMaterializer
{
    private readonly EntityMetadata _entityMetadata;
    private readonly IReadOnlyDictionary<ColumnMetadata, Table> _fKRelations;

    public EntityMaterializer(
        EntityMetadata entityMetadata,
        IReadOnlyDictionary<ColumnMetadata, Table> fKRelations
    )
    {
        _entityMetadata = entityMetadata;
        _fKRelations = fKRelations;
    }

    public object FromBuffer(ValueBuffer buffer)
    {
        ValueBuffer tempBuffer = new();

        foreach ((ColumnMetadata column, object columnValue) in buffer)
        {
            if (
                column.Reference == null
                || column.Reference != column.DataType
                || columnValue.Equals(DBNull.Value)
            )
            {
                tempBuffer[column] = columnValue;
                continue;
            }

            Table depTable = _fKRelations[column];
            ValueBuffer depBuffer = depTable.GetRowByPrimaryKey(columnValue);
            tempBuffer[column] = depTable.Materializer.FromBuffer(depBuffer);
        }

        return _entityMetadata.Binding.MaterializeEntity(tempBuffer);
    }
}
