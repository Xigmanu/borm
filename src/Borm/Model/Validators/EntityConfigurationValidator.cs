using System.Diagnostics;
using Borm.Properties;
using Borm.Reflection;

namespace Borm.Model.Validators;

internal sealed class EntityConfigurationValidator<TEntity> : IValidator<IReadOnlyList<MappingMember>>
    where TEntity : class
{
    public void Validate(IReadOnlyList<MappingMember> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        string entityName = typeof(TEntity).FullName!;
        if (properties.Count == 0)
        {
            throw new ArgumentException(Strings.EmptyColumnCollection(entityName));
        }

        List<MappingMember> primaryKeys =
        [
            .. properties.Where(c => c.Mapping?.IsPrimaryKey == true),
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
        IReadOnlyList<MappingMember> properties,
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
                    $"Duplicate column index '{mapping.ColumnIndex}' found in entity '{entityName}'."
                );
            }
            if (mapping.ColumnName != null && !columnNames.Add(mapping.ColumnName))
            {
                throw new InvalidOperationException(
                    $"Duplicate column name '{mapping.ColumnName}' found in entity '{entityName}'."
                );
            }
        }
    }
}
