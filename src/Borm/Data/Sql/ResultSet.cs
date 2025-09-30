using System.Data.Common;
using System.Diagnostics;

namespace Borm.Data.Sql;

/// <summary>
/// Represents a forward-only, row-based result set returned from a database query.
/// </summary>
public sealed class ResultSet
{
    private readonly List<IReadOnlyDictionary<string, object>> _rows = [];
    private int _cursor = -1;

    /// <summary>
    /// Gets the row at the current cursor position.
    /// </summary>
    ///
    /// <exception cref="InvalidOperationException">
    /// Thrown if the cursor is not positioned on a valid row.
    /// </exception>
    public IReadOnlyDictionary<string, object> Current
    {
        get
        {
            if (_cursor < 0 || _cursor >= _rows.Count)
            {
                throw new InvalidOperationException("Cursor is not positioned on a valid row."); // TODO
            }
            return _rows[_cursor];
        }
    }

    /// <summary>
    /// Number of rows in the result set.
    /// </summary>
    public int RowCount => _rows.Count;

    public static ResultSet FromReader(DbDataReader reader)
    {
        Debug.Assert(!reader.IsClosed);

        ResultSet resultSet = new();
        IEnumerable<string> columnNames = reader.GetColumnSchema().Select(c => c.ColumnName);

        while (reader.Read())
        {
            Dictionary<string, object> row = new(StringComparer.OrdinalIgnoreCase);
            foreach (string column in columnNames)
            {
                row[column] = reader[column];
            }
            resultSet.AddRow(row);
        }

        return resultSet;
    }

    /// <summary>
    /// Advances the cursor to the next row in the result set.
    /// </summary>
    ///
    /// <returns>
    /// <see langword="true"/> if the cursor was successfully advanced to the next row;
    /// <see langword="false"/> if there are no more rows.
    /// </returns>
    public bool MoveNext()
    {
        return ++_cursor < _rows.Count;
    }

    internal void AddRow(IReadOnlyDictionary<string, object> row)
    {
        _rows.Add(row);
    }
}
