using System.Diagnostics;
using Borm.Model.Construction;
using Borm.Properties;
using Borm.Reflection;
using Borm.Util;

namespace Borm.Model.Validators;

internal sealed class ColumnConfigurationValidator<TEntity>
    : IValidator<ColumnBuilder<TEntity>.Configuration>
    where TEntity : class
{
    public void Validate(ColumnBuilder<TEntity>.Configuration configuration)
    {
        if (string.IsNullOrWhiteSpace(configuration.PropertyName))
        {
            throw new InvalidOperationException();
        }

        NullableType? dataType = configuration.DataType;
        Debug.Assert(dataType != null);

        Type? reference = configuration.Reference;

        if (configuration.IsPrimaryKey)
        {
            if (reference != null)
            {
                throw new InvalidOperationException("Primary key cannot be a foreign key");
            }
            if (dataType.IsNullable)
            {
                throw new InvalidOperationException(
                    $"Primary key cannot be nullable. Entity: {typeof(TEntity).FullName}"
                );
            }
        }
        if (reference == null && !ColumnDataTypeHelper.IsSupported(dataType.UnderlyingType))
        {
            throw new NotSupportedException(
                Strings.TypeNotSupported(dataType.UnderlyingType.FullName!)
            );
        }
    }
}
