using System.Collections;
using System.Data;
using System.Runtime.CompilerServices;

namespace Borm.Schema.Metadata;

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

    public static ValueBuffer FromDataRow(EntityNode node, DataRow row)
    {
        ValueBuffer buffer = new();
        foreach (ColumnInfo column in node.Columns)
        {
            buffer[column] = row[column.Name];
        }
        return buffer;
    }

    public IEnumerator<KeyValuePair<ColumnInfo, object>> GetEnumerator()
    {
        return _valueMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void LoadIntoRow(DataRow row)
    {
        foreach (KeyValuePair<ColumnInfo, object> keyValue in _valueMap)
        {
            row[keyValue.Key.Name] = keyValue.Value;
        }
    }
}
