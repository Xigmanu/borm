using System.Data;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data.Storage;

internal class ConstraintValidator
{
    private readonly Table _table;

    public ConstraintValidator(Table table)
    {
        _table = table;
    }

    public void ValidateBuffer(ValueBuffer buffer, long txId)
    {
        foreach ((ColumnMetadata column, object columnValue) in buffer)
        {
            ValidateConstraints(column, columnValue, txId);
        }
    }

    private void ValidateConstraints(ColumnMetadata column, object columnValue, long txId)
    {
        Constraints constraints = column.Constraints;

        if (!constraints.HasFlag(Constraints.AllowDbNull) && columnValue == DBNull.Value)
        {
            throw new ConstraintException(
                Strings.NullableConstraintViolation(column.Name, _table.Name)
            );
        }
        if (
            constraints.HasFlag(Constraints.Unique)
            && !_table.Tracker.IsColumnValueUnique(column, columnValue, txId)
        )
        {
            throw new ConstraintException(
                Strings.UniqueConstraintViolation(_table.Name, column.Name, columnValue)
            );
        }
    }
}
