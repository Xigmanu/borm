using System.Collections.ObjectModel;

namespace Borm.Data.Sql.Sqlite.Tests;

public class SqliteCommandFactoryTest
{
    private static readonly TableInfo AddressesTableSchema = CreateSimpleTableSchema();
    private static readonly TableInfo PersonsTableSchema = CreateRelationalTableSchema();

    [Fact]
    public void NewCreateTableStatement_ReturnsSqliteCreateTableStatement_WithSimpleTable()
    {
        // Arrange
        string expectedSql =
            "CREATE TABLE addresses(id INTEGER PRIMARY KEY,address TEXT NOT NULL,address_1 TEXT NULL,city TEXT NOT NULL);";
        TableInfo table = AddressesTableSchema;
        SqliteCommandDefinitionFactory commandFactory = new();

        // Act
        DbCommandDefinition actual = commandFactory.CreateTable(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Empty(actual.Parameters);
    }

    [Fact]
    public void NewCreateTableStatement_ReturnsSqliteCreateTableStatement_WithTableWithRelation()
    {
        // Arrange
        string expectedSql =
            "CREATE TABLE persons(id INTEGER PRIMARY KEY,name TEXT UNIQUE NOT NULL,salary REAL NOT NULL,address INTEGER NULL REFERENCES addresses(id));";
        TableInfo table = PersonsTableSchema;
        SqliteCommandDefinitionFactory commandFactory = new();

        // Act
        DbCommandDefinition actual = commandFactory.CreateTable(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Empty(actual.Parameters);
    }

    [Fact]
    public void NewDeleteStatement_ReturnsSqliteDeleteStatement_WithSimpleTable()
    {
        // Arrange
        string expectedSql = "DELETE FROM addresses WHERE id = $id;";
        TableInfo table = AddressesTableSchema;
        string expectedPKName = DbCommandDefinition.DefaultParameterPrefix + table.Columns[0].Name;
        SqliteCommandDefinitionFactory commandFactory = new();

        // Act
        DbCommandDefinition actual = commandFactory.Delete(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Single(actual.Parameters);
        Assert.Equal(expectedPKName, actual.Parameters[^1].ParameterName);
    }

    [Fact]
    public void NewInsertStatement_ReturnsSqliteInsertStatement_WithSimpleTable()
    {
        // Arrange
        string expectedSql = "INSERT INTO addresses VALUES($id,$address,$address_1,$city);";
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
        string expectedSql = "SELECT * FROM addresses;";
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
        string expectedSql =
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
        IEnumerable<ColumnInfo> columns,
        int offset = 0
    )
    {
        string[] res = new string[columns.Count() - offset];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = DbCommandDefinition.DefaultParameterPrefix + columns.ElementAt(i + offset).Name;
        }
        return res;
    }

    private static TableInfo CreateRelationalTableSchema()
    {
        List<ColumnInfo> columns =
        [
            new ColumnInfo("id", typeof(int), false, false),
            new ColumnInfo("name", typeof(string), true, false),
            new ColumnInfo("salary", typeof(double), false, false),
            new ColumnInfo("address", typeof(int), false, true),
        ];
        return new TableInfo(
            "persons",
            new ReadOnlyCollection<ColumnInfo>(columns),
            columns[0],
            new Dictionary<ColumnInfo, TableInfo>() { [columns[^1]] = AddressesTableSchema }.AsReadOnly()
        );
    }

    private static TableInfo CreateSimpleTableSchema()
    {
        List<ColumnInfo> columns =
        [
            new ColumnInfo("id", typeof(int), false, false),
            new ColumnInfo("address", typeof(string), false, false),
            new ColumnInfo("address_1", typeof(string), false, true),
            new ColumnInfo("city", typeof(string), false, false),
        ];
        return new TableInfo(
            "addresses",
            new ReadOnlyCollection<ColumnInfo>(columns),
            columns[0],
            ReadOnlyDictionary<ColumnInfo, TableInfo>.Empty
        );
    }
}
