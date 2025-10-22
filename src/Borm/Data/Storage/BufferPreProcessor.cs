using System.Diagnostics;
using System.Runtime.CompilerServices;
using Borm.Model.Metadata;

namespace Borm.Data.Storage;

internal sealed class BufferPreProcessor
{
    private readonly TableGraph _graph;

    public BufferPreProcessor(TableGraph graph)
    {
        _graph = graph;
    }

    public List<ResolvedForeignKey> ResolveForeignKeys(
        IValueBuffer buffer,
        long txId,
        out IValueBuffer processed
    )
    {
        processed = new ValueBuffer();
        List<ResolvedForeignKey> resolvedKeys = [];
        foreach ((ColumnMetadata column, object columnValue) in buffer)
        {
            if (!IsValueSimple(column, columnValue))
            {
                ResolvedForeignKey key = ResolveKey(column, columnValue, txId);
                Debug.Assert(key.PrimaryKey != null);
                processed[column] = key.PrimaryKey;
                resolvedKeys.Add(key);
                continue;
            }

            processed[column] = columnValue;
        }

        return resolvedKeys;
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValueSimple(ColumnMetadata column, object columnValue)
    {
        return column.Reference == null || columnValue.Equals(DBNull.Value);
    }

    private ResolvedForeignKey ResolveKey(ColumnMetadata column, object columnValue, long txId)
    {
        Table? parent = _graph[column.Reference!];
        Debug.Assert(parent is not null);

        bool changeExists;
        EntityMetadata metadata = parent.Metadata;
        if (column.DataType != metadata.DataType)
        {
            changeExists = parent.Tracker.TryGetChange(primaryKey: columnValue, txId, out _);
            return new ResolvedForeignKey(
                parent,
                columnValue,
                columnValue,
                IsComplexRecord: false,
                changeExists
            );
        }

        IValueBuffer parentBuffer = metadata.Conversion.ToValueBuffer(columnValue);
        object primaryKey = parentBuffer.PrimaryKey;

        changeExists = parent.Tracker.TryGetChange(primaryKey, txId, out _);
        return new ResolvedForeignKey(
            parent,
            primaryKey,
            columnValue,
            IsComplexRecord: true,
            changeExists
        );
    }
}
