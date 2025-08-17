using System.Data;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

internal sealed class EntityConverter
{
    private readonly Table _table;

    public EntityConverter(Table table)
    {
        _table = table;
    }

    public object Materialize(ValueBuffer buffer)
    {
        ValueBuffer tempBuffer = new();

        foreach ((ColumnMetadata column, object columnValue) in buffer)
        {
            bool isSimpleValue =
                column.Reference == null
                || column.Reference != column.DataType
                || columnValue.Equals(DBNull.Value);

            tempBuffer[column] = !isSimpleValue
                ? ResolveDependency(_table.ForeignKeyRelations[column], columnValue)
                : columnValue;
        }

        return _table.EntityMetadata.Binding.MaterializeEntity(tempBuffer);
    }

    public ValueBuffer ResolveForeignKeyValues(
        ValueBuffer incoming,
        long txId,
        bool isRecursiveInsert
    )
    {
        ValueBuffer result = new();

        foreach ((ColumnMetadata column, object columnValue) in incoming)
        {
            ValidateConstraints(column, columnValue, txId);

            bool isSimpleValue = column.Reference == null || columnValue.Equals(DBNull.Value);
            result[column] = !isSimpleValue
                ? Resolve(column, columnValue, txId, isRecursiveInsert)
                : columnValue;
        }

        return result;
    }

    private static object ResolveDependency(Table dependency, object columnValue)
    {
        // The initial TX ID is used to ensure that I only read committed changes
        if (
            dependency.Tracker.TryGetChange(
                columnValue,
                InternalTransaction.InitId,
                out Change? change
            )
        )
        {
            return dependency.Converter.Materialize(change.Buffer);
        }

        return DBNull.Value;
    }

    private object Resolve(
        ColumnMetadata column,
        object columnValue,
        long txId,
        bool isRecursiveInsert
    )
    {
        Table dependencyTable = _table.ForeignKeyRelations[column];
        EntityMetadata depNode = dependencyTable.EntityMetadata;
        if (column.DataType != depNode.DataType)
        {
            if (!dependencyTable.Tracker.TryGetChange(primaryKey: columnValue, txId, out _))
            {
                throw new RowNotFoundException(
                    Strings.RowNotFound(_table.Name, columnValue),
                    _table.Name,
                    columnValue
                );
            }
            return columnValue;
        }

        ValueBuffer fkBuffer = depNode.Binding.ToValueBuffer(columnValue);
        object depPrimaryKey = fkBuffer.PrimaryKey;

        bool changeExists = dependencyTable.Tracker.TryGetChange(depPrimaryKey, txId, out _);
        if (isRecursiveInsert && !changeExists)
        {
            dependencyTable.Insert(columnValue, txId);
        }

        return depPrimaryKey;
    }

    private void ValidateConstraints(ColumnMetadata column, object columnValue, long txId)
    {
        Constraints constraints = column.Constraints;

        if (!constraints.HasFlag(Constraints.AllowDbNull) && columnValue == null)
        {
            throw new ConstraintException(
                Strings.NullableConstraintViolation(column.Name, _table.Name)
            );
        }
        if (
            constraints.HasFlag(Constraints.Unique)
            && _table.Tracker.IsColumnValueUnique(column, columnValue, txId)
        )
        {
            throw new ConstraintException(
                Strings.UniqueConstraintViolation(_table.Name, column.Name, columnValue)
            );
        }
    }
}
