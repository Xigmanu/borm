
using Borm.Model;

namespace Borm.Data.Sql.Sqlite.Tests.Mocks;

internal sealed class TestColumn : IColumn
{
    public TestColumn(string name, Type dataType, Constraints constraints)
    {
        Name = name;
        DataType = dataType;
        Constraints = constraints;
    }

    public string Name { get; }

    public Type DataType { get; }

    public Constraints Constraints { get; }
}

