using System.Collections;
using System.Collections.ObjectModel;

namespace Borm.Model.Metadata;

internal sealed class ColumnMetadataList : IReadOnlyList<ColumnMetadata>
{
    private readonly Dictionary<string, ColumnMetadata> _byNameMap;
    private readonly ReadOnlyCollection<ColumnMetadata> _columns;

    public ColumnMetadataList(IEnumerable<ColumnMetadata> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        _columns = new ReadOnlyCollection<ColumnMetadata>([.. columns]);
        _byNameMap = columns.ToDictionary(c => c.Name);
    }

    public int Count => _columns.Count;

    public ColumnMetadata this[string columnName]
    {
        get =>
            _byNameMap.TryGetValue(columnName, out ColumnMetadata? column)
                ? column
                : throw new KeyNotFoundException($"Column {columnName} not found");
    }

    public ColumnMetadata this[int idx]
    {
        get => _columns[idx];
    }

    public bool Contains(ColumnMetadata item)
    {
        return _columns.Contains(item);
    }

    public IEnumerator<ColumnMetadata> GetEnumerator()
    {
        return _columns.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
