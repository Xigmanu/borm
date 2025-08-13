using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Borm.Model.Metadata;

internal sealed class EntityInfoValidator
{
    private readonly IEnumerable<EntityInfo> _entityInfos;

    public EntityInfoValidator(IEnumerable<EntityInfo> entityInfos)
    {
        _entityInfos = entityInfos;
    }

    public bool IsValid(EntityInfo entityInfo, [NotNullWhen(false)] out Exception? exception)
    {
        exception = ValidatePrimaryKeyColumn(entityInfo);
        if (exception != null)
        {
            return false;
        }

        foreach (ColumnInfo columnInfo in entityInfo.Columns)
        {
            exception = ValidateColumnIndex(entityInfo, columnInfo);
            if (exception != null)
            {
                return false;
            }

            if (columnInfo.Reference != null)
            {
                exception = ValidateForeignKeyColumn(entityInfo, columnInfo);
                if (exception != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static InvalidOperationException? ValidateColumnIndex(
        EntityInfo entityInfo,
        ColumnInfo columnInfo
    )
    {
        bool isIndexValid = entityInfo.Columns.Count > columnInfo.Index && columnInfo.Index >= 0;
        return isIndexValid
            ? null
            : new InvalidOperationException(
                $"Invalid column index {columnInfo.Index} in entity {entityInfo.DataType.FullName}. Valid range is [0, {entityInfo.Columns.Count})"
            );
    }

    private static Exception? ValidatePrimaryKeyColumn(EntityInfo entityInfo)
    {
        IEnumerable<ColumnInfo> primaryKeys = entityInfo.Columns.Where(column =>
            column.Constraints.HasFlag(Constraints.PrimaryKey)
        );
        if (!primaryKeys.Any())
        {
            return new MissingPrimaryKeyException(
                $"Entity {entityInfo.DataType.FullName} has no primary key"
            );
        }
        if (primaryKeys.Count() > 1)
        {
            return new InvalidOperationException(
                $"Entity {entityInfo.DataType.FullName} has multiple primary keys"
            );
        }

        ColumnInfo primaryKey = primaryKeys.First();
        if (primaryKey.Constraints.HasFlag(Constraints.AllowDbNull))
        {
            return new InvalidOperationException(
                $"Primary key cannot be nullable. Entity: {entityInfo.DataType.FullName}"
            );
        }

        return null;
    }

    private InvalidOperationException? ValidateForeignKeyColumn(
        EntityInfo entityInfo,
        ColumnInfo columnInfo
    )
    {
        Type dataType = columnInfo.Constraints.HasFlag(Constraints.AllowDbNull)
            ? columnInfo.DataType
            : columnInfo.PropertyType;

        Type reference = columnInfo.Reference!;
        EntityInfo? successor = _entityInfos.FirstOrDefault(m => m.DataType.Equals(reference));
        if (successor == null)
        {
            return new NodeNotFoundException(
                $"Referenced entity type {reference.FullName} does not exist",
                reference
            );
        }

        bool isFKValid =
            dataType.Equals(columnInfo.Reference) || dataType.Equals(successor.PrimaryKey.DataType);

        return isFKValid
            ? null
            : new InvalidOperationException(
                $"The foreign key property must be of the referenced type or the type of its primary key. Entity: {entityInfo.DataType.FullName}"
            );
    }
}
