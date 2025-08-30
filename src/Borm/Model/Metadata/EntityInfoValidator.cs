using System.Data;
using System.Diagnostics.CodeAnalysis;
using Borm.Properties;
using Borm.Util;

namespace Borm.Model.Metadata;

internal sealed class EntityInfoValidator
{
    private readonly IEnumerable<EntityMetadata> _entityMetadata;

    public EntityInfoValidator(IEnumerable<EntityMetadata> entityMetadata)
    {
        _entityMetadata = entityMetadata;
    }

    public bool IsValid(
        EntityMetadata entityMetadata,
        [NotNullWhen(false)] out Exception? exception
    )
    {
        exception = ValidatePrimaryKeyColumn(entityMetadata);
        if (exception != null)
        {
            return false;
        }

        foreach (ColumnMetadata column in entityMetadata.Columns)
        {
            exception = ValidateColumnIndex(entityMetadata, column);
            if (exception != null)
            {
                return false;
            }

            if (column.Reference != null)
            {
                exception = ValidateForeignKeyColumn(entityMetadata, column);
                return exception == null;
            }

            if (!ColumnDataTypeHelper.IsSupported(column.DataType))
            {
                throw new NotSupportedException(
                    Strings.TypeNotSupported(column.DataType.FullName!)
                );
            }
        }

        return true;
    }

    private static InvalidOperationException? ValidateColumnIndex(
        EntityMetadata entityMetadata,
        ColumnMetadata column
    )
    {
        bool isIndexValid = entityMetadata.Columns.Count > column.Index && column.Index >= 0;
        return isIndexValid
            ? null
            : new InvalidOperationException(
                $"Invalid column index {column.Index} in entity {entityMetadata.DataType.FullName}. Valid range is [0, {entityMetadata.Columns.Count})"
            );
    }

    private static Exception? ValidatePrimaryKeyColumn(EntityMetadata entityMetadata)
    {
        IEnumerable<ColumnMetadata> primaryKeys = entityMetadata.Columns.Where(column =>
            column.Constraints.HasFlag(Constraints.PrimaryKey)
        );
        if (!primaryKeys.Any())
        {
            return new MissingPrimaryKeyException(
                $"Entity {entityMetadata.DataType.FullName} has no primary key"
            );
        }
        if (primaryKeys.Count() > 1)
        {
            return new InvalidOperationException(
                $"Entity {entityMetadata.DataType.FullName} has multiple primary keys"
            );
        }

        ColumnMetadata primaryKey = primaryKeys.First();
        if (primaryKey.Constraints.HasFlag(Constraints.AllowDbNull))
        {
            return new InvalidOperationException(
                $"Primary key cannot be nullable. Entity: {entityMetadata.DataType.FullName}"
            );
        }

        return null;
    }

    private InvalidOperationException? ValidateForeignKeyColumn(
        EntityMetadata entityMetadata,
        ColumnMetadata column
    )
    {
        Type dataType = column.Constraints.HasFlag(Constraints.AllowDbNull)
            ? column.DataType
            : column.PropertyType;

        Type reference = column.Reference!;
        EntityMetadata? successor = _entityMetadata.FirstOrDefault(m =>
            m.DataType.Equals(reference)
        );
        if (successor == null)
        {
            return new NodeNotFoundException(
                $"Referenced entity type {reference.FullName} does not exist",
                reference
            );
        }

        bool isFKValid =
            dataType.Equals(column.Reference) || dataType.Equals(successor.PrimaryKey.DataType);

        return isFKValid
            ? null
            : new InvalidOperationException(
                $"The foreign key property must be of the referenced type or the type of its primary key. Entity: {entityMetadata.DataType.FullName}"
            );
    }
}
