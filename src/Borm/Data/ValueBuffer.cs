using System.Collections;
using System.Diagnostics;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Data;

internal sealed class ValueBuffer : IEnumerable<KeyValuePair<ColumnMetadata, object>>
{
    private readonly Dictionary<ColumnMetadata, object> _valueMap = [];
    private ColumnMetadata? _primaryKey;

    public object PrimaryKey
    {
        get
        {
            Debug.Assert(_primaryKey != null);
            return _valueMap[_primaryKey];
        }
    }

    public object this[ColumnMetadata column]
    {
        get => _valueMap[column];
        set
        {
            if (column.Constraints.HasFlag(Constraints.PrimaryKey))
            {
                Debug.Assert(_primaryKey == null);
                _primaryKey = column;
            }
            _valueMap[column] = value;
        }
    }

    public object this[string columnName] =>
        _valueMap.First(kvp => kvp.Key.Name == columnName).Value;

    public IEnumerator<KeyValuePair<ColumnMetadata, object>> GetEnumerator()
    {
        return _valueMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
