using System.Data;

namespace Borm.Data.Sql.Sqlite.Test;

public class SqliteStatementFactoryTest
{
    [Fact]
    public void NewCreateTableStatement_ReturnsSqliteCreateTableStatement_WithDataTableWithRelation()
    {
        // Arrange
        string expectedSql =
            "CREATE TABLE relational(id TEXT PRIMARY KEY,comment TEXT NULL,simple_fk TEXT NOT NULL REFERENCES simple(id));";
        DataTable dataTable = CreateTestDataSet().Tables[1];
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewCreateTableStatement(dataTable);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Empty(actual.Parameters);
    }

    [Fact]
    public void NewCreateTableStatement_ReturnsSqliteCreateTableStatement_WithSimpleDataTable()
    {
        // Arrange
        string expectedSql =
            "CREATE TABLE simple(id TEXT PRIMARY KEY,name TEXT NOT NULL,num_entries INTEGER NOT NULL,amount REAL NOT NULL);";
        DataTable dataTable = CreateTestDataSet().Tables[0];
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewCreateTableStatement(dataTable);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Empty(actual.Parameters);
    }

    [Fact]
    public void NewDeleteStatement_ReturnsSqliteDeleteStatement_WithSimpleDataTable()
    {
        // Arrange
        string expectedSql = "DELETE FROM simple WHERE id = $id;";
        DataTable dataTable = CreateTestDataSet().Tables[0];
        string expectedPKName =
            SqlStatement.DefaultParameterPrefix + dataTable.Columns[0].ColumnName;
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewDeleteStatement(dataTable);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Single(actual.Parameters);
        Assert.Equal(expectedPKName, actual.Parameters[^1].ParameterName);
    }

    [Fact]
    public void NewInsertStatement_ReturnsSqliteInsertStatement_WithSimpleDataTable()
    {
        // Arrange
        string expectedSql = "INSERT INTO simple VALUES($id,$name,$num_entries,$amount);";
        DataTable dataTable = CreateTestDataSet().Tables[0];
        string[] expectedParamNames = CreateExpectedParameterNames(dataTable.Columns);
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewInsertStatement(dataTable);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Equal(dataTable.Columns.Count, actual.Parameters.Length);
        for (int i = 0; i < actual.Parameters.Length; i++)
        {
            Assert.Equal(expectedParamNames[i], actual.Parameters[i].ParameterName);
        }
    }

    [Fact]
    public void NewSelectAllStatement_ReturnsSqliteSelectAllStatement_WithSimpleDataTable()
    {
        // Arrange
        string expectedSql = "SELECT * FROM simple;";
        DataTable dataTable = CreateTestDataSet().Tables[0];
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewSelectAllStatement(dataTable);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Empty(actual.Parameters);
    }

    [Fact]
    public void NewUpdateStatement_ReturnsSqliteUpdateStatement_WithSimpleDataTable()
    {
        // Arrange
        string expectedSql =
            "UPDATE simple SET name = $name,num_entries = $num_entries,amount = $amount WHERE id = $id;";
        DataTable dataTable = CreateTestDataSet().Tables[0];
        string[] expectedParamNames = CreateExpectedParameterNames(dataTable.Columns, 1);
        SqliteStatementFactory statementFactory = new();

        // Act
        SqlStatement actual = statementFactory.NewUpdateStatement(dataTable);

        // Assert
        Assert.Equal(expectedSql, actual.Sql);
        Assert.Equal(dataTable.Columns.Count, actual.Parameters.Length);
        for (int i = 1; i < expectedParamNames.Length; i++)
        {
            Assert.Equal(expectedParamNames[i], actual.Parameters[i].ParameterName);
        }
        Assert.Equal(
            SqlStatement.DefaultParameterPrefix + dataTable.PrimaryKey[0].ColumnName,
            actual.Parameters[^1].ParameterName
        );
    }

    private static string[] CreateExpectedParameterNames(
        DataColumnCollection columns,
        int offset = 0
    )
    {
        string[] res = new string[columns.Count - offset];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = SqlStatement.DefaultParameterPrefix + columns[i + offset].ColumnName;
        }
        return res;
    }

    private static DataSet CreateTestDataSet()
    {
        DataTable dataTable0 = new("simple");
        DataColumn first0 = new("id", typeof(Guid)) { AllowDBNull = false };
        DataColumn second0 = new("name", typeof(string)) { AllowDBNull = false };
        DataColumn third0 = new("num_entries", typeof(int)) { AllowDBNull = false };
        DataColumn fourth0 = new("amount", typeof(double)) { AllowDBNull = false };

        dataTable0.Columns.AddRange([first0, second0, third0, fourth0]);
        dataTable0.PrimaryKey = [first0];

        DataTable dataTable1 = new("relational");
        DataColumn first1 = new("id", typeof(Guid)) { AllowDBNull = false };
        DataColumn second1 = new("comment", typeof(string));
        DataColumn third1 = new("simple_fk", typeof(Guid)) { AllowDBNull = false };

        dataTable1.Columns.AddRange([first1, second1, third1]);
        dataTable1.PrimaryKey = [first1];

        DataSet dataSet = new("test");
        dataSet.Tables.AddRange([dataTable0, dataTable1]);
        dataSet.Relations.Add("relational_simple", dataTable0.PrimaryKey[0], dataTable1.Columns[2]);

        return dataSet;
    }
}
