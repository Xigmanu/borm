using System.Data;
using System.Runtime.CompilerServices;
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

    public void ValidateBuffer(
        IEnumerable<KeyValuePair<IColumnMetadata, object>> buffer,
        long txId,
        [CallerMemberName] string? method = null
    )
    {
        foreach ((IColumnMetadata column, object columnValue) in buffer)
        {
            ValidateConstraints(column, columnValue, txId, method == nameof(_table.Update));
        }
    }

    private void ValidateConstraints(
        IColumnMetadata column,
        object columnValue,
        long txId,
        bool isUpdate
    )
    {
        Constraints constraints = column.Constraints;

        if (!constraints.HasFlag(Constraints.AllowDbNull) && columnValue == DBNull.Value)
        {
            throw new ConstraintException(
                Strings.NullableConstraintViolation(column.Name, _table.Name)
            );
        }

        bool isUniqueViolated =
            constraints.HasFlag(Constraints.Unique)
            && !isUpdate
            && !_table.Tracker.IsColumnValueUnique(column, columnValue, txId);
        if (isUniqueViolated)
        {
            throw new ConstraintException(
                Strings.UniqueConstraintViolation(_table.Name, column.Name, columnValue)
            );
        }
    }
}
