using Borm.Model.Metadata;

namespace Borm.Data;

internal sealed class EntityMaterializer
{
    private readonly EntityInfo _entityInfo;
    private readonly IReadOnlyDictionary<IColumn, ITable> _fKRelations;

    public EntityMaterializer(
        EntityInfo entityInfo,
        IReadOnlyDictionary<IColumn, ITable> fKRelations
    )
    {
        _entityInfo = entityInfo;
        _fKRelations = fKRelations;
    }

    public object FromBuffer(ValueBuffer buffer)
    {
        ValueBuffer tempBuffer = new();

        foreach ((ColumnInfo column, object columnValue) in buffer)
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

            Table depTable = (Table)_fKRelations[column];
            ValueBuffer depBuffer = depTable.GetRowByPrimaryKey(columnValue);
            tempBuffer[column] = depTable.Materializer.FromBuffer(depBuffer);
        }

        return _entityInfo.Binding.MaterializeEntity(tempBuffer);
    }
}
