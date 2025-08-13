using Borm.Tests.Common;

namespace Borm.Data.Sql.Sqlite.Tests;

public class SqliteStatementFactoryTest
{
    [Fact]
    public void NewCreateTableStatement_ReturnsSqliteCreateTableStatement_WithSimpleTable()
    {
        // Arrange
        string expectedSql =
            "CREATE TABLE addresses(id INTEGER PRIMARY KEY,address TEXT NOT NULL,address_1 TEXT NULL,city TEXT NOT NULL);";
        TestTable table = Mocks.AddressesTable;
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewCreateTableStatement(table);

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
        TestTable table = Mocks.PersonsTable;
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewCreateTableStatement(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Empty(actual.Parameters);
    }

    [Fact]
    public void NewDeleteStatement_ReturnsSqliteDeleteStatement_WithSimpleTable()
    {
        // Arrange
        string expectedSql = "DELETE FROM addresses WHERE id = $id;";
        TestTable table = Mocks.AddressesTable;
        string expectedPKName = SqlStatement.DefaultParameterPrefix + table.Columns.First().Name;
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewDeleteStatement(table);

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
        TestTable table = Mocks.AddressesTable;
        string[] expectedParamNames = CreateExpectedParameterNames(table.Columns);
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewInsertStatement(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Equal(table.Columns.Count(), actual.Parameters.Length);
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
        TestTable table = Mocks.AddressesTable;
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewSelectAllStatement(table);

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
        TestTable table = Mocks.AddressesTable;
        string[] expectedParamNames = CreateExpectedParameterNames(table.Columns, 1);
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewUpdateStatement(table);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Equal(table.Columns.Count(), actual.Parameters.Length);
        for (int i = 1; i < expectedParamNames.Length; i++)
        {
            Assert.Equal(expectedParamNames[i], actual.Parameters[i].ParameterName);
        }
        Assert.Equal(
            SqlStatement.DefaultParameterPrefix + table.PrimaryKey.Name,
            actual.Parameters[^1].ParameterName
        );
    }

    private static string[] CreateExpectedParameterNames(
        IEnumerable<IColumn> columns,
        int offset = 0
    )
    {
        string[] res = new string[columns.Count() - offset];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = SqlStatement.DefaultParameterPrefix + columns.ElementAt(i + offset).Name;
        }
        return res;
    }
}
