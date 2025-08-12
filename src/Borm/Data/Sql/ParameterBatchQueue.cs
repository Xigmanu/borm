using System.Data;
using System.Diagnostics;
using Borm.Model.Metadata;

namespace Borm.Data.Sql;

public sealed class ParameterBatchQueue
{
    private readonly List<ValueBuffer> _values;
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
        ValueBuffer buffer = _values[_currentIdx];
        IDataParameterCollection parameters = dbCommand.Parameters;
        for (int i = 0; i < parameters.Count; i++)
        {
            IDbDataParameter? param = parameters[i] as IDbDataParameter;
            Debug.Assert(param != null);
            string columnName = param.ParameterName[1..]; // remove the prefix
            param.Value = buffer[columnName];
        }
    }

    internal void AddFromChange(Change entry)
    {
        _values.Add(entry.Buffer);
    }
}
