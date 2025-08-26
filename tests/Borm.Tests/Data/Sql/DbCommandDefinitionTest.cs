using System.Data;
using System.Data.Common;
using Borm.Data.Sql;
using Moq;

namespace Borm.Tests.Data.Sql;

public sealed class DbCommandDefinitionTest
{
    [Fact]
    public void Constructor_ShouldInitializeBatchQueue()
    {
        // Act
        DbCommandDefinition command = new(string.Empty, []);

        // Assert
        Assert.NotNull(command.BatchQueue);
    }

    [Fact]
    public void Prepare_ShouldSetCommandTextAndAddParameters()
    {
        // Arrange
        string sql = "INSERT INTO person VALUES ($id, $name);";
        DbParameter[] parameters =
        [
            CreateParameter("id", DbType.Int32),
            CreateParameter("name", DbType.String),
        ];

        DbCommandDefinition command = new(sql, parameters);

        Mock<IDbCommand> mockCommand = new();
        Mock<IDataParameterCollection> mockParams = new();

        int addCalls = 0;

        mockParams.Setup(p => p.Clear());
        mockParams
            .Setup(p => p.Add(It.IsAny<IDbDataParameter>()))
            .Callback<object>(_ => addCalls++);

        mockCommand.Setup(c => c.Parameters).Returns(mockParams.Object);
        mockCommand.SetupProperty(c => c.CommandText);
        mockCommand.Setup(c => c.Prepare());

        // Act
        command.Prepare(mockCommand.Object);

        // Assert
        Assert.Equal(sql, mockCommand.Object.CommandText);
        mockParams.Verify(p => p.Clear(), Times.Once);
        Assert.Equal(2, addCalls);
        mockCommand.Verify(c => c.Prepare(), Times.Once);
    }

    private static DbParameter CreateParameter(string name, DbType type)
    {
        Mock<DbParameter> mockParam = new();
        mockParam.SetupAllProperties();
        mockParam.Object.ParameterName = name;
        mockParam.Object.DbType = type;
        return mockParam.Object;
    }
}
