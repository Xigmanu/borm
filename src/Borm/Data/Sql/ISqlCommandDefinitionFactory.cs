namespace Borm.Data.Sql;

public interface ISqlCommandDefinitionFactory
{
    DbCommandDefinition CreateTable(TableInfo tableSchema);
    DbCommandDefinition Delete(TableInfo tableSchema);

    DbCommandDefinition Insert(TableInfo tableSchema);

    DbCommandDefinition SelectAll(TableInfo tableSchema);
    DbCommandDefinition Update(TableInfo tableSchema);
}
