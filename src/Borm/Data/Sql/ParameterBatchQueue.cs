using Borm.Data.Storage;
using System.Data;
using System.Diagnostics;

namespace Borm.Data.Sql;

/// <summary>
/// Represents a queue of parameter value sets that can be applied to a database command for batch execution.
/// </summary>
public sealed class ParameterBatchQueue
{
    private readonly Queue<ValueBuffer> _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterBatchQueue"/> class.
    /// </summary>
    public ParameterBatchQueue()
    {
        _values = [];
    }

    /// <summary>
    /// Number of parameter sets currently on the queue.
    /// </summary>
    public int Count => _values.Count;

    /// <summary>
    /// Determines whether the queue contains another set of parameter values.
    /// </summary>
    /// <returns><see langword="true"/> if at least one set of parameter values is available; otherwise, <see langword="false"/>.</returns>
    public bool HasNext()
    {
        return _values.TryPeek(out _);
    }

    /// <summary>
    /// Dequeues the next set of parameter values and applies them to the specified
    /// <see cref="IDbCommand"/>.
    /// </summary>
    /// <param name="dbCommand">The database command whose parameters will be updated.</param>
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
