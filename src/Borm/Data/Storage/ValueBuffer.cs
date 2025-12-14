using System.Collections;
using System.Diagnostics;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Data.Storage;

[DebuggerDisplay("PrimaryKey = {PrimaryKey}")]
internal sealed class ValueBuffer : IValueBuffer
{
    private readonly Dictionary<IColumnMetadata, object> _valueMap;
    private IColumnMetadata? _primaryKey;

    public ValueBuffer()
    {
        _valueMap = [];
    }

    private ValueBuffer(ValueBuffer original)
    {
        _valueMap = original._valueMap.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        _primaryKey = original._primaryKey;
    }

    public int Length => _valueMap.Count;

    public object PrimaryKey
    {
        get
        {
            Debug.Assert(_primaryKey != null);
            return _valueMap[_primaryKey];
        }
    }

    public object this[IColumnMetadata column]
    {
        get => _valueMap[column];
        set
        {
            Debug.Assert(value != null);
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

    public IValueBuffer Copy()
    {
        return new ValueBuffer(this);
    }

    public IEnumerator<KeyValuePair<IColumnMetadata, object>> GetEnumerator()
    {
        return _valueMap.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueBuffer other
               && other._valueMap.SequenceEqual(_valueMap, new KeyValuePairEqualityComparer());
    }

    public override int GetHashCode()
    {
        return _valueMap.GetHashCode();
    }

    private sealed class KeyValuePairEqualityComparer
        : IEqualityComparer<KeyValuePair<IColumnMetadata, object>>
    {
        public bool Equals(
            KeyValuePair<IColumnMetadata, object> x,
            KeyValuePair<IColumnMetadata, object> y
        )
        {
            return Equals(x.Key, y.Key) && Equals(x.Value, y.Value);
        }

        public int GetHashCode(KeyValuePair<IColumnMetadata, object> obj)
        {
            return HashCode.Combine(obj.Key, obj.Value);
        }
    }
}