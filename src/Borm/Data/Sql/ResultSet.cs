namespace Borm.Data.Sql;

public sealed class ResultSet
{
    private readonly List<IReadOnlyDictionary<string, object>> _rows = [];
    private int _cursor = -1;

    public int RowCount => _rows.Count;

    public bool MoveNext()
    {
        return ++_cursor < _rows.Count;
    }

    public IReadOnlyDictionary<string, object> Current
    {
        get
        {
            if (_cursor < 0 || _cursor >= _rows.Count)
            {
                throw new InvalidOperationException(); // TODO
            }
            return _rows[_cursor];
        }
    }

    internal void AddRow(IReadOnlyDictionary<string, object> row)
    {
        _rows.Add(row);
    }
}
