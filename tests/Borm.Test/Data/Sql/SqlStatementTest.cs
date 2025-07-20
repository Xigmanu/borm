using System.Data;
using System.Data.Common;
using Borm.Data.Sql;
using Moq;

namespace Borm.Tests.Data.Sql;

public sealed class SqlStatementTest
{
    private static DbParameter CreateParameter(string name, object? value)
    {
        Mock<DbParameter> mockParam = new();
        mockParam.SetupAllProperties();
        mockParam.Object.ParameterName = name;
        mockParam.Object.Value = value;
        return mockParam.Object;
    }
}
