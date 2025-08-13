using Borm.Model;

namespace Borm.Data.Sql.Sqlite.Tests.Mocks;

internal sealed class TestTable : ITable
{
    public static readonly TestTable AddressesTable = CreateAddressesTable();
    public static readonly TestTable PersonsTable = CreatePersonsTable();

    public TestTable(
        IEnumerable<IColumn> columns,
        string name,
        IColumn primaryKey,
        IReadOnlyDictionary<IColumn, ITable> foreignKeyRelations
    )
    {
        Columns = columns;
        Name = name;
        PrimaryKey = primaryKey;
        ForeignKeyRelations = foreignKeyRelations;
    }

    public IEnumerable<IColumn> Columns { get; }

    public IReadOnlyDictionary<IColumn, ITable> ForeignKeyRelations { get; }
    public string Name { get; }

    public IColumn PrimaryKey { get; }

    private static TestTable CreateAddressesTable()
    {
        string tableNameA = "addresses";
        List<IColumn> columnsA =
        [
            new TestColumn("id", typeof(int), Constraints.PrimaryKey),
            new TestColumn("address", typeof(string), Constraints.None),
            new TestColumn("address_1", typeof(string), Constraints.AllowDbNull),
            new TestColumn("city", typeof(string), Constraints.None),
        ];

        return new TestTable(columnsA, tableNameA, columnsA[0], new Dictionary<IColumn, ITable>());
    }

    private static TestTable CreatePersonsTable()
    {
        string tableNameB = "persons";
        List<IColumn> columnsB =
        [
            new TestColumn("id", typeof(int), Constraints.PrimaryKey),
            new TestColumn("name", typeof(string), Constraints.Unique),
            new TestColumn("salary", typeof(double), Constraints.None),
            new TestColumn("address", typeof(int), Constraints.AllowDbNull),
        ];

        Dictionary<IColumn, ITable> fkRelations = new() { [columnsB[^1]] = AddressesTable };

        return new TestTable(columnsB, tableNameB, columnsB[0], fkRelations);
    }
}
