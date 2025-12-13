using System.Diagnostics;
using System.Runtime.CompilerServices;
using Borm.Data.Storage;
using Borm.Model.Metadata;

namespace Borm.Data.Internal;

internal sealed class RecordPreProcessor : IRecordPreProcessor
{
    private readonly ITableGraph _graph;

    public RecordPreProcessor(ITableGraph graph)
    {
        _graph = graph;
    }

    public IValueBuffer Process(
        IValueBuffer record,
        long txId,
        out IEnumerable<ResolvedForeignKey> keys
    )
    {
        ValueBuffer processed = new();
        List<ResolvedForeignKey> resolvedKeys = [];
        foreach ((IColumnMetadata column, object columnValue) in record)
        {
            if (!IsValueSimple(column, columnValue))
            {
                ResolvedForeignKey key = ResolveKey(column, columnValue, txId);
                Debug.Assert(key.Value != null);
                processed[column] = key.Value;
                resolvedKeys.Add(key);
                continue;
            }

            processed[column] = columnValue;
        }

        keys = resolvedKeys.AsReadOnly();
        return processed;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValueSimple(IColumnMetadata column, object columnValue)
    {
        return column.Reference == null || columnValue.Equals(DBNull.Value);
    }

    private ResolvedForeignKey ResolveKey(IColumnMetadata column, object columnValue, long txId)
    {
        ITable? parent = _graph[column.Reference!];
        Debug.Assert(parent is not null);

        bool changeExists;
        IEntityMetadata metadata = parent.Metadata;
        if (column.DataType.UnderlyingType != metadata.Type)
        {
            changeExists = parent.Tracker.TryGetChange(columnValue, txId, out _);
            return new ResolvedForeignKey(parent, columnValue, columnValue, false, changeExists);
        }

        IValueBuffer parentBuffer = metadata.Conversion.ToValueBuffer(columnValue);
        object primaryKey = parentBuffer.PrimaryKey;

        changeExists = parent.Tracker.TryGetChange(primaryKey, txId, out _);
        return new ResolvedForeignKey(parent, primaryKey, columnValue, true, changeExists);
    }
}