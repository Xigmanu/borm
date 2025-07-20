using System.Data;
using System.Data.Common;

namespace Borm.Data.Sql;

public sealed class ParameterBatchQueue
{
    private readonly List<object?[]> _values;
    private int _currentIdx;

    public ParameterBatchQueue()
    {
        _values = [];
        _currentIdx = 0;
    }

    public int Count => _values.Count;

    public bool Next()
    {
        return _values.Count > _currentIdx;
    }

    public void SetDbParameters(DbCommand dbCommand)
    {
        object?[] values = _values[_currentIdx];
        for (int i = 0; i < values.Length; i++)
        {
            dbCommand.Parameters[i].Value = values[i];
        }
        _currentIdx++;
    }

    internal void AddFromRow(DataRow row)
    {
        DataTable table = row.Table;
        DataColumnCollection columns = table.Columns;
        object?[] values = new object[columns.Count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = row[i];
        }
        _values.Add(values);
    }
}
