namespace Borm.Data.Sql;

internal sealed class InMemoryCommandDefinitionFactory : IDbCommandDefinitionFactory
{
    public DbCommandDefinition CreateTable(TableInfo tableSchema)
    {
        return DbCommandDefinition.Empty;
    }

    public DbCommandDefinition Delete(TableInfo tableSchema)
    {
        return DbCommandDefinition.Empty;
    }

    public DbCommandDefinition Insert(TableInfo tableSchema)
    {
        return DbCommandDefinition.Empty;
    }

    public DbCommandDefinition SelectAll(TableInfo tableSchema)
    {
        return DbCommandDefinition.Empty;
    }

    public DbCommandDefinition Update(TableInfo tableSchema)
    {
        return DbCommandDefinition.Empty;
    }
}
