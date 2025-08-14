using System.Collections;
using System.Runtime.CompilerServices;

namespace Borm.Model.Metadata;

internal sealed class ColumnInfoCollection : IReadOnlyCollection<Column>
{
    private readonly Column[] _columns;

    public ColumnInfoCollection(IEnumerable<Column> columns)
    {
        _columns = [.. columns];
    }

    public int Count => _columns.Length;

    public Column this[string columnName]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.First(column => column.Name == columnName);
    }

    public IEnumerator<Column> GetEnumerator()
    {
        for (int i = 0; i < _columns.Length; i++)
        {
            yield return _columns[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
