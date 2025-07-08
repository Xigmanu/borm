using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Borm.Schema.Metadata;

internal sealed class TableNodeValidator
{
    private readonly List<TableNode> _nodes;

    public TableNodeValidator(List<TableNode> nodes)
    {
        _nodes = nodes;
    }

    public bool IsValid(TableNode node, [NotNullWhen(false)] out Exception? exception)
    {
        exception = ValidatePrimaryKeyColumn(node);
        if (exception != null)
        {
            return false;
        }

        foreach (ColumnInfo columnInfo in node.Columns)
        {
            exception = ValidateColumnIndex(node, columnInfo);
            if (exception != null)
            {
                return false;
            }

            if (columnInfo.ReferencedEntityType != null)
            {
                exception = ValidateForeignKeyColumn(node, columnInfo);
                if (exception != null)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private static Type UnwrapNullableType(Type type)
    {
        if (!type.IsValueType)
        {
            return type;
        }

        Type? underlying = Nullable.GetUnderlyingType(type);
        Debug.Assert(underlying != null);

        return underlying;
    }

    private static InvalidOperationException? ValidateColumnIndex(
        TableNode node,
        ColumnInfo columnInfo
    )
    {
        bool isIndexValid = node.Columns.Count() > columnInfo.Index && columnInfo.Index >= 0;
        return isIndexValid
            ? null
            : new InvalidOperationException(
                $"Invalid column index {columnInfo.Index} in entity {node.DataType.FullName}. Valid range is [0, {node.Columns.Count()})"
            );
    }

    private static Exception? ValidatePrimaryKeyColumn(TableNode node)
    {
        IEnumerable<ColumnInfo> primaryKeys = node.Columns.Where(column =>
            column.Constraints.HasFlag(Constraints.PrimaryKey)
        );
        if (!primaryKeys.Any())
        {
            return new MissingPrimaryKeyException(
                $"Entity {node.DataType.FullName} has no primary key"
            );
        }
        if (primaryKeys.Count() > 1)
        {
            return new InvalidOperationException(
                $"Entity {node.DataType.FullName} has multiple primary keys"
            );
        }

        ColumnInfo primaryKey = primaryKeys.First();
        if (primaryKey.Constraints.HasFlag(Constraints.AllowDbNull))
        {
            return new InvalidOperationException(
                $"Primary key cannot be nullable. Entity: {node.DataType.FullName}"
            );
        }

        return null;
    }

    private InvalidOperationException? ValidateForeignKeyColumn(
        TableNode node,
        ColumnInfo columnInfo
    )
    {
        Type dataType = columnInfo.Constraints.HasFlag(Constraints.AllowDbNull)
            ? UnwrapNullableType(columnInfo.DataType)
            : columnInfo.DataType;

        TableNode? successor = _nodes.Find(node =>
            node.DataType.Equals(columnInfo.ReferencedEntityType)
        );
        if (successor == null)
        {
            return new InvalidOperationException(
                $"Referenced entity type {columnInfo.ReferencedEntityType!.FullName} does not exist"
            );
        }

        bool isFKValid =
            dataType.Equals(columnInfo.ReferencedEntityType)
            || dataType.Equals(successor.GetPrimaryKey().DataType);

        return isFKValid
            ? null
            : new InvalidOperationException(
                $"The foreign key property must be of the referenced type or the type of its primary key. Entity: {node.DataType.FullName}"
            );
    }
}
