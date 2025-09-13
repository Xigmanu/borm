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

    public ValueBuffer ResolveForeignKeys(
        ValueBuffer buffer,
        long txId,
        out List<ResolvedForeignKey> resolvedKeys
    )
    {
        ValueBuffer resolved = new();
        resolvedKeys = [];
        foreach ((ColumnMetadata column, object columnValue) in buffer)
        {
            if (!IsValueSimple(column, columnValue))
            {
                ResolvedForeignKey key = ResolveKey(column, columnValue, txId);
                Debug.Assert(key.Value != null);
                resolved[column] = key.Value;
                resolvedKeys.Add(key);
                continue;
            }

            resolved[column] = columnValue;
        }

        return resolved;
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
        EntityMetadata metadata = parent.EntityMetadata;
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

        ValueBuffer parentBuffer = metadata.Binding.ToValueBuffer(columnValue);
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
