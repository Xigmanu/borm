using System.Diagnostics;
using Borm.Model.Metadata;
using Borm.Properties;

namespace Borm.Data.Storage;

internal sealed class EntityMaterializer
{
    private readonly Table _table;

    public EntityMaterializer(Table table)
    {
        _table = table;
    }

    public object Materialize(ValueBuffer buffer)
    {
        ValueBuffer tempBuffer = new();

        foreach ((ColumnMetadata column, object columnValue) in buffer)
        {
            bool isSimpleValue =
                column.Reference == null
                || column.Reference != column.DataType
                || columnValue.Equals(DBNull.Value);

            tempBuffer[column] = isSimpleValue
                ? columnValue
                : MaterializeDependency(_table.ForeignKeyRelations[column], columnValue);
        }

        return _table.EntityMetadata.Binding.MaterializeEntity(tempBuffer);
    }

    public IEnumerable<ResolvedForeignKey> ResolveForeignKeys(ValueBuffer incoming, long txId)
    {
        List<ResolvedForeignKey> result = [];
        foreach ((ColumnMetadata column, object columnValue) in incoming)
        {
            bool isSimpleValue = column.Reference == null || columnValue.Equals(DBNull.Value);
            if (!isSimpleValue)
            {
                object foreignKey = ResolveForeignKey(
                    column,
                    columnValue,
                    txId,
                    out bool changeExists
                );
                Debug.Assert(foreignKey != null);
                result.Add(new ResolvedForeignKey(column, columnValue, foreignKey, changeExists));
            }
        }

        return result;
    }

    private static object MaterializeDependency(Table dependency, object columnValue)
    {
        // The initial TX ID is used to ensure that I only read committed changes
        bool changeExists = dependency.Tracker.TryGetChange(
            columnValue,
            InternalTransaction.InitId,
            out Change? change
        );
        if (changeExists)
        {
            return dependency.Converter.Materialize(change!.Buffer);
        }

        return DBNull.Value;
    }

    private object ResolveForeignKey(
        ColumnMetadata column,
        object columnValue,
        long txId,
        out bool changeExists
    )
    {
        Table dependencyTable = _table.ForeignKeyRelations[column];
        EntityMetadata depNode = dependencyTable.EntityMetadata;
        if (column.DataType != depNode.DataType)
        {
            changeExists = dependencyTable.Tracker.TryGetChange(
                primaryKey: columnValue,
                txId,
                out _
            );
            if (!changeExists)
            {
                throw new RowNotFoundException(
                    Strings.RowNotFound(_table.Name, columnValue),
                    _table.Name,
                    columnValue
                );
            }
            return columnValue;
        }

        ValueBuffer fkBuffer = depNode.Binding.ToValueBuffer(columnValue);
        object depPrimaryKey = fkBuffer.PrimaryKey;

        changeExists = dependencyTable.Tracker.TryGetChange(depPrimaryKey, txId, out _);
        return depPrimaryKey;
    }

    internal sealed record ResolvedForeignKey(
        ColumnMetadata Column,
        object RawValue,
        object ResolvedKey,
        bool ChangeExists
    );
}
