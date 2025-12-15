using System.Collections.ObjectModel;

namespace Borm.Data.Sql.Sqlite.Tests;

public class SqliteCommandFactoryTest
{
    private static readonly TableInfo AddressesTableSchema = CreateSimpleTableSchema();

    [Fact]
    public void NewDeleteStatement_ReturnsSqliteDeleteStatement_WithSimpleTable()
    {
        // Arrange
        const string expectedSql = "DELETE FROM addresses WHERE id = $id;";
        TableInfo table = AddressesTableSchema;
        string expectedPkName = DbCommandDefinition.DefaultParameterPrefix + table.Columns[0].Name;
        SqliteCommandDefinitionFactory commandFactory = new();

        // Act
        DbCommandDefinition actual = commandFactory.Delete(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Single(actual.Parameters);
        Assert.Equal(expectedPkName, actual.Parameters[^1].ParameterName);
    }

    [Fact]
    public void NewInsertStatement_ReturnsSqliteInsertStatement_WithSimpleTable()
    {
        // Arrange
        const string expectedSql = "INSERT INTO addresses VALUES($id,$address,$address_1,$city);";
        TableInfo table = AddressesTableSchema;
        string[] expectedParamNames = CreateExpectedParameterNames(table.Columns);
        SqliteCommandDefinitionFactory commandFactory = new();

        // Act
        DbCommandDefinition actual = commandFactory.Insert(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Equal(table.Columns.Count, actual.Parameters.Length);
        for (int i = 0; i < actual.Parameters.Length; i++)
        {
            Assert.Equal(expectedParamNames[i], actual.Parameters[i].ParameterName);
        }
    }

    [Fact]
    public void NewSelectAllStatement_ReturnsSqliteSelectAllStatement_WithSimpleTable()
    {
        // Arrange
        const string expectedSql = "SELECT * FROM addresses;";
        TableInfo table = AddressesTableSchema;
        SqliteCommandDefinitionFactory commandFactory = new();

        // Act
        DbCommandDefinition actual = commandFactory.SelectAll(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Empty(actual.Parameters);
    }

    [Fact]
    public void NewUpdateStatement_ReturnsSqliteUpdateStatement_WithSimpleTable()
    {
        // Arrange
        const string expectedSql =
            "UPDATE addresses SET address = $address,address_1 = $address_1,city = $city WHERE id = $id;";
        TableInfo table = AddressesTableSchema;
        string[] expectedParamNames = CreateExpectedParameterNames(table.Columns, 1);
        SqliteCommandDefinitionFactory commandFactory = new();

        // Act
        DbCommandDefinition actual = commandFactory.Update(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Equal(table.Columns.Count, actual.Parameters.Length);
        for (int i = 1; i < expectedParamNames.Length; i++)
        {
            Assert.Equal(expectedParamNames[i], actual.Parameters[i].ParameterName);
        }

        Assert.Equal(
            DbCommandDefinition.DefaultParameterPrefix + table.PrimaryKey.Name,
            actual.Parameters[^1].ParameterName
        );
    }

    private static string[] CreateExpectedParameterNames(
        IReadOnlyCollection<ColumnInfo> columns,
        int offset = 0
    )
    {
        string[] res = new string[columns.Count - offset];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] =
                DbCommandDefinition.DefaultParameterPrefix + columns.ElementAt(i + offset).Name;
        }

        return res;
    }

    private static TableInfo CreateSimpleTableSchema()
    {
        List<ColumnInfo> columns =
        [
            new("id", "persons", typeof(int), false, false),
            new("address", "persons", typeof(string), false, false),
            new("address_1", "persons", typeof(string), false, true),
            new("city", "persons", typeof(string), false, false)
        ];
        return new TableInfo(
            "addresses",
            new ReadOnlyCollection<ColumnInfo>(columns),
            columns[0],
            ReadOnlyDictionary<ColumnInfo, TableInfo>.Empty
        );
    }
}