using System.Data;
using System.Diagnostics;

namespace Borm.Data.Sql;

public sealed class ParameterBatchQueue
{
    private readonly Queue<ValueBuffer> _values;

    public ParameterBatchQueue()
    {
        _values = [];
    }

    public int Count => _values.Count;

    public bool HasNext()
    {
        return _values.TryPeek(out _);
    }

    public void SetParameterValues(IDbCommand dbCommand)
    {
        ValueBuffer buffer = _values.Dequeue();
        IDataParameterCollection parameters = dbCommand.Parameters;
        for (int i = 0; i < parameters.Count; i++)
        {
            IDbDataParameter? param = parameters[i] as IDbDataParameter;
            Debug.Assert(param != null);
            string columnName = param.ParameterName[1..]; // remove the prefix
            param.Value = buffer[columnName];
        }
    }

    internal void Enqueue(ValueBuffer buffer)
    {
        _values.Enqueue(buffer);
    }
}
