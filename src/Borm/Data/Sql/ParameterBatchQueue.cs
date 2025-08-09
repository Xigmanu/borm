using System.Data;
using System.Diagnostics;
using Borm.Model.Metadata;

namespace Borm.Data.Sql;

public sealed class ParameterBatchQueue
{
    private readonly List<object?[]> _values;
    private int _currentIdx;

    public ParameterBatchQueue()
    {
        _values = [];
        _currentIdx = -1;
    }

    public int Count => _values.Count;

    public bool Next()
    {
        _currentIdx++;
        return _values.Count > _currentIdx;
    }

    public void SetParameterValues(IDbCommand dbCommand)
    {
        object?[] values = _values[_currentIdx];
        for (int i = 0; i < values.Length; i++)
        {
            IDbDataParameter? param = dbCommand.Parameters[i] as IDbDataParameter;
            Debug.Assert(param != null);
            param.Value = values[i];
        }
    }

    internal void AddFromChange(Change entry)
    {
        ValueBuffer buffer = entry.Buffer;
        object?[] bufArr = buffer.ToColumnOrderedArray();
        _values.Add(bufArr);
    }
}
