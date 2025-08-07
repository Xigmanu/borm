using System.Collections;

namespace Borm.Data;

internal sealed class TableSet : IEnumerable<Table>
{
    private readonly HashSet<Table> _tables = [];

    public void Add(Table table)
    {
        _tables.Add(table);
    }

    public IEnumerator<Table> GetEnumerator()
    {
        return _tables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
