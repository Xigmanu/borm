using System.Data;
using Borm.Model;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data;

internal sealed class ConstraintChecker
{
    private readonly string _tableName;
    private readonly ChangeTracker _tracker;

    public ConstraintChecker(ChangeTracker tracker, string tableName)
    {
        _tracker = tracker;
        _tableName = tableName;
    }

    public void Check(ColumnInfo column, object columnValue, long txId)
    {
        Constraints constraints = column.Constraints;

        if (!constraints.HasFlag(Constraints.AllowDbNull) && columnValue == null)
        {
            throw new ConstraintException(Strings.NullableConstraintViolation(column.Name, _tableName));
        }
        if (
            constraints.HasFlag(Constraints.Unique) && _tracker.HasChange(column, columnValue, txId)
        )
        {
            throw new ConstraintException(
                Strings.UniqueConstraintViolation(_tableName, column.Name, columnValue)
            );
        }
    }
}
