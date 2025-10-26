using System.Data;
using System.Diagnostics.CodeAnalysis;
using Borm.Properties;
using Borm.Util;

namespace Borm.Model.Metadata;

internal sealed class EntityMetadataValidator
{
    private readonly IEnumerable<IEntityMetadata> _entityMetadata;

    public EntityMetadataValidator(IEnumerable<IEntityMetadata> entityMetadata)
    {
        _entityMetadata = entityMetadata;
    }

    public bool IsValid(
        IEntityMetadata entityMetadata,
        [NotNullWhen(false)] out Exception? exception
    )
    {
        exception = ValidatePrimaryKeyColumn(entityMetadata);
        if (exception != null)
        {
            return false;
        }

        foreach (IColumnMetadata column in entityMetadata.Columns)
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

            if (!ColumnDataTypeHelper.IsSupported(column.DataType.UnderlyingType))
            {
                throw new NotSupportedException(
                    Strings.TypeNotSupported(column.DataType.FullName!)
                );
            }
        }

        return true;
    }

    private static InvalidOperationException? ValidateColumnIndex(
        IEntityMetadata entityMetadata,
        IColumnMetadata column
    )
    {
        bool isIndexValid = entityMetadata.Columns.Count > column.Index && column.Index >= 0;
        return isIndexValid
            ? null
            : new InvalidOperationException(
                $"Invalid column index {column.Index} in entity {entityMetadata.Type.FullName}. Valid range is [0, {entityMetadata.Columns.Count})"
            );
    }

    private static Exception? ValidatePrimaryKeyColumn(IEntityMetadata entityMetadata)
    {
        IEnumerable<IColumnMetadata> primaryKeys = entityMetadata.Columns.Where(column =>
            column.Constraints.HasFlag(Constraints.PrimaryKey)
        );
        if (!primaryKeys.Any())
        {
            return new MissingPrimaryKeyException(
                $"Entity {entityMetadata.Type.FullName} has no primary key"
            );
        }
        if (primaryKeys.Count() > 1)
        {
            return new InvalidOperationException(
                $"Entity {entityMetadata.Type.FullName} has multiple primary keys"
            );
        }

        IColumnMetadata primaryKey = primaryKeys.First();
        if (primaryKey.Constraints.HasFlag(Constraints.AllowDbNull))
        {
            return new InvalidOperationException(
                $"Primary key cannot be nullable. Entity: {entityMetadata.Type.FullName}"
            );
        }

        return null;
    }

    private InvalidOperationException? ValidateForeignKeyColumn(
        IEntityMetadata entityMetadata,
        IColumnMetadata column
    )
    {
        Type dataType = column.DataType.UnderlyingType;
        Type reference = column.Reference!;
        IEntityMetadata? successor = _entityMetadata.FirstOrDefault(m => m.Type.Equals(reference));
        if (successor == null)
        {
            return new EntityNotFoundException(
                $"Referenced entity type {reference.FullName} does not exist",
                reference
            );
        }

        bool isFKValid =
            dataType.Equals(column.Reference)
            || dataType.Equals(successor.PrimaryKey.DataType.UnderlyingType);

        return isFKValid
            ? null
            : new InvalidOperationException(
                $"The foreign key property must be of the referenced type or the type of its primary key. Entity: {entityMetadata.Type.FullName}"
            );
    }
}
