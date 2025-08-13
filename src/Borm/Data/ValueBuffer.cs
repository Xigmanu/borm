using Borm.Model;
using Borm.Model.Metadata;
using System.Collections;

namespace Borm.Data;

internal sealed class ValueBuffer : IEnumerable<KeyValuePair<ColumnInfo, object>>
{
    private readonly Dictionary<ColumnInfo, object> _valueMap = [];

    public object this[ColumnInfo column]
    {
        get => _valueMap[column];
        set => _valueMap[column] = value;
    }

    public object this[string columnName] =>
        _valueMap.First(kvp => kvp.Key.Name == columnName).Value;

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
