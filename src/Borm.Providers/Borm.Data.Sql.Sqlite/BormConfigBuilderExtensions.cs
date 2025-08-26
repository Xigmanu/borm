using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite;

public static class BormConfigBuilderExtensions
{
    public static BormConfig.Builder Sqlite(
        this BormConfig.Builder builder,
        SqliteConnectionStringBuilder connStrBuilder
    )
    {
        SqliteCommandExecutor executor = new(connStrBuilder.ToString());
        SqliteCommandDefinitionFactory factory = new();
        return builder.CommandExecutor(executor).CommandDefinitionFactory(factory);
    }
}
