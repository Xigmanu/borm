using System.Collections;
using System.Runtime.CompilerServices;

namespace Borm.Model.Metadata;

internal sealed class ColumnMetadataCollection : IReadOnlyCollection<ColumnMetadata>
{
    private readonly ColumnMetadata[] _columns;

    public ColumnMetadataCollection(IEnumerable<ColumnMetadata> columns)
    {
        _columns = [.. columns];
    }

    public int Count => _columns.Length;

    public ColumnMetadata this[string columnName]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this.First(column => column.Name == columnName);
    }

    public IEnumerator<ColumnMetadata> GetEnumerator()
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
