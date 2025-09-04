namespace Borm.Data.Sql;

/// <summary>
/// Defines a factory for creating <see cref="DbCommandDefinition"/> instances.
/// </summary>
public interface IDbCommandDefinitionFactory
{
    /// <summary>
    /// Creates a <see cref="DbCommandDefinition"/> that defines
    /// the command to create a new table according to the specified schema.
    /// </summary>
    DbCommandDefinition CreateTable(TableInfo tableSchema);

    /// <summary>
    /// Creates a <see cref="DbCommandDefinition"/> that defines
    /// the command to delete records from the specified table.
    /// </summary>
    DbCommandDefinition Delete(TableInfo tableSchema);

    /// <summary>
    /// Creates a <see cref="DbCommandDefinition"/> that defines
    /// the command to insert new records into the specified table.
    /// </summary>
    DbCommandDefinition Insert(TableInfo tableSchema);

    /// <summary>
    /// Creates a <see cref="DbCommandDefinition"/> that defines the command
    /// to query all records from the specified table.
    /// </summary>
    DbCommandDefinition SelectAll(TableInfo tableSchema);

    /// <summary>
    /// Creates a <see cref="DbCommandDefinition"/> that defines the command
    /// to update records in the specified table.
    /// </summary>
    DbCommandDefinition Update(TableInfo tableSchema);
}
