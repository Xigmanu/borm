using System.Collections;
using System.Collections.ObjectModel;

namespace Borm.Model.Metadata;

internal sealed class ColumnMetadataList : IReadOnlyList<IColumnMetadata>
{
    private readonly Dictionary<string, IColumnMetadata> _byNameMap;
    private readonly ReadOnlyCollection<IColumnMetadata> _columns;

    public ColumnMetadataList(IEnumerable<IColumnMetadata> columns)
    {
        ArgumentNullException.ThrowIfNull(columns);

        _columns = new ReadOnlyCollection<IColumnMetadata>([.. columns]);
        _byNameMap = columns.ToDictionary(c => c.Name);
    }

    public int Count => _columns.Count;

    public IColumnMetadata this[string columnName]
    {
        get =>
            _byNameMap.TryGetValue(columnName, out IColumnMetadata? column)
                ? column
                : throw new KeyNotFoundException($"Column {columnName} not found");
    }

    public IColumnMetadata this[int idx]
    {
        get => _columns[idx];
    }

    public bool Contains(IColumnMetadata item)
    {
        return _columns.Contains(item);
    }

    public IEnumerator<IColumnMetadata> GetEnumerator()
    {
        return _columns.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
