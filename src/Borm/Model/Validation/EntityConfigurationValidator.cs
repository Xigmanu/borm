using System.Diagnostics;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Validation;

internal sealed class EntityConfigurationValidator<TEntity>
    : IConfigurationValidator<IReadOnlyList<IMappable>>
    where TEntity : class
{
    public void Validate(IReadOnlyList<IMappable> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        string entityName = typeof(TEntity).FullName!;
        if (properties.Count == 0)
        {
            throw new ArgumentException(Strings.EmptyColumnCollection(entityName));
        }

        List<IMappable> primaryKeys =
        [
            .. properties.Where(c => c.Mapping?.IsPrimaryKey == true)
        ];
        switch (primaryKeys.Count)
        {
            case 0:
                throw new InvalidOperationException(Strings.MissingPrimaryKey(entityName));
            case > 1:
                throw new InvalidOperationException(Strings.MultiplePrimaryKeys(entityName));
        }

        ValidateColumnIdentifiers(properties, entityName);
    }

    private static void ValidateColumnIdentifiers(
        IReadOnlyList<IMappable> properties,
        string entityName
    )
    {
        HashSet<int> columnIndexes = [];
        HashSet<string> columnNames = [];
        foreach (MappingInfo? mapping in properties.Select(prop => prop.Mapping))
        {
            Debug.Assert(mapping != null);

            if (!columnIndexes.Add(mapping.ColumnIndex))
            {
                throw new InvalidOperationException(
                    Strings.DuplicateColumnIndex(mapping.ColumnIndex, entityName)
                );
            }

            if (mapping.ColumnName != null && !columnNames.Add(mapping.ColumnName))
            {
                throw new InvalidOperationException(
                    Strings.DuplicateColumnName(mapping.ColumnName, entityName)
                );
            }
        }
    }
}