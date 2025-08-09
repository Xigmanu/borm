using System.Collections;
using System.Runtime.CompilerServices;

namespace Borm.Model.Metadata;

internal sealed class ValueBuffer : IEnumerable<KeyValuePair<ColumnInfo, object>>
{
    private readonly Dictionary<ColumnInfo, object> _valueMap = [];

    public object this[ColumnInfo column]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _valueMap[column];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _valueMap[column] = value;
    }

    public object[] ToColumnOrderedArray()
    {
        List<object> ret = new(_valueMap.Count);
        IEnumerable<ColumnInfo> ordered = _valueMap.Keys.OrderBy(col => col.Index);
        foreach (ColumnInfo column in ordered)
        {
            ret.Add(_valueMap[column]);
        }

        return [.. ret];
    }

    public IEnumerator<KeyValuePair<ColumnInfo, object>> GetEnumerator()
    {
        return _valueMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public object GetPrimaryKey()
    {
        return _valueMap
            .First(keyVal => keyVal.Key.Constraints.HasFlag(Constraints.PrimaryKey))
            .Value!;
    }
}
