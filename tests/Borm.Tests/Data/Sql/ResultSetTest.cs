using System.Data;
using Borm.Data.Sql;

namespace Borm.Tests.Data.Sql;

public sealed class ResultSetTest
{
    [Fact]
    public void Current_ReturnsCurrentRow()
    {
        // Arrange
        ResultSet resultSet = new();
        Dictionary<string, object> row = new() { ["test"] = 42 };
        resultSet.AddRow(row);
        resultSet.MoveNext();

        // Act
        IReadOnlyDictionary<string, object> current = resultSet.Current;

        // Assert
        Assert.Equal(1, resultSet.RowCount);
        Assert.Single(row);
        Assert.Equal(row.Count, current.Count);
        Assert.Equal(row["test"], current["test"]);
    }

    [Fact]
    public void FromReader_ReadsRowsIntoResultSet()
    {
        // Arrange
        DataTable table = new();

        string idColName = "id";
        string nameColName = "name";
        table.Columns.Add(idColName, typeof(int));
        table.Columns.Add(nameColName, typeof(string));

        table.Rows.Add(1, "foo");
        table.Rows.Add(2, "bar");

        TestDataReader reader = new(table);

        // Act
        ResultSet resultSet = ResultSet.FromReader(reader);

        // Assert
        Assert.Equal(table.Rows.Count, resultSet.RowCount);

        foreach (DataRow tableRow in table.Rows)
        {
            _ = resultSet.MoveNext();
            IReadOnlyDictionary<string, object> row = resultSet.Current;
            Assert.Equal(tableRow[idColName], row[idColName]);
            Assert.Equal(tableRow[nameColName], row[nameColName]);
        }
    }

    // ReSharper disable UnassignedGetOnlyAutoProperty
    private sealed class TestDataReader : IDataReader
    {
        private readonly DataTable _table;
        private int _cursor;

        public TestDataReader(DataTable table)
        {
            _table = table;
            _cursor = -1;
        }

        public int Depth { get; }

        public int FieldCount { get; }

        public bool IsClosed => false;

        public int RecordsAffected { get; }

        public object this[int i] => throw new NotImplementedException();

        public object this[string name]
        {
            get
            {
                DataColumn column = _table.Columns[name]!;
                DataRow row = _table.Rows[_cursor];
                return row[column];
            }
        }

        public void Close() => throw new NotImplementedException();

        public void Dispose() => throw new NotImplementedException();

        public bool GetBoolean(int i) => throw new NotImplementedException();

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(
            int i,
            long fieldOffset,
            byte[]? buffer,
            int bufferoffset,
            int length
        ) => throw new NotImplementedException();

        public char GetChar(int i) => throw new NotImplementedException();

        public long GetChars(
            int i,
            long fieldoffset,
            char[]? buffer,
            int bufferoffset,
            int length
        ) => throw new NotImplementedException();

        public IDataReader GetData(int i) => throw new NotImplementedException();

        public string GetDataTypeName(int i) => throw new NotImplementedException();

        public DateTime GetDateTime(int i) => throw new NotImplementedException();

        public decimal GetDecimal(int i) => throw new NotImplementedException();

        public double GetDouble(int i) => throw new NotImplementedException();

        public Type GetFieldType(int i) => throw new NotImplementedException();

        public float GetFloat(int i) => throw new NotImplementedException();

        public Guid GetGuid(int i) => throw new NotImplementedException();

        public short GetInt16(int i) => throw new NotImplementedException();

        public int GetInt32(int i) => throw new NotImplementedException();

        public long GetInt64(int i) => throw new NotImplementedException();

        public string GetName(int i) => throw new NotImplementedException();

        public int GetOrdinal(string name) => throw new NotImplementedException();

        public DataTable GetSchemaTable() => _table;

        public string GetString(int i) => throw new NotImplementedException();

        public object GetValue(int i) => throw new NotImplementedException();

        public int GetValues(object[] values) => throw new NotImplementedException();

        public bool IsDBNull(int i) => throw new NotImplementedException();

        public bool NextResult() => throw new NotImplementedException();

        public bool Read()
        {
            if (_cursor >= _table.Rows.Count - 1)
            {
                return false;
            }

            _cursor++;
            return true;
        }
    }
    // ReSharper restore UnassignedGetOnlyAutoProperty
}