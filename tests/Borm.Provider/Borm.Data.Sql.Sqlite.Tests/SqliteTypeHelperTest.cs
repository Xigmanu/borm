using Microsoft.Data.Sqlite;

namespace Borm.Data.Sql.Sqlite.Tests;

public class SqliteTypeHelperTest
{
    public static readonly IEnumerable<object[]> IntegerTypesData =
    [
        [typeof(ushort)],
        [typeof(short)],
        [typeof(ulong)],
        [typeof(long)],
        [typeof(uint)],
        [typeof(int)],
    ];
    public static readonly IEnumerable<object[]> RealTypesData =
    [
        [typeof(float)],
        [typeof(double)],
        [typeof(decimal)],
    ];
    public static readonly IEnumerable<object[]> TextTypesData =
    [
        [typeof(char)],
        [typeof(bool)],
        [typeof(string)],
        [typeof(Guid)],
    ];

    [Theory]
    [MemberData(nameof(IntegerTypesData))]
    public void ToSqliteType_ReturnsIntegerSqliteType_WithIntegerTypes(Type integerType)
    {
        // Arrange
        SqliteType expected = SqliteType.Integer;

        // Act
        SqliteType actual = SqliteTypeHelper.ToSqliteType(integerType);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(RealTypesData))]
    public void ToSqliteType_ReturnsRealSqliteType_WithFloatTypes(Type floatType)
    {
        // Arrange
        SqliteType expected = SqliteType.Real;

        // Act
        SqliteType actual = SqliteTypeHelper.ToSqliteType(floatType);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [MemberData(nameof(TextTypesData))]
    public void ToSqliteType_ReturnsTextSqliteType_WithStringableTypes(Type stringableType)
    {
        // Arrange
        SqliteType expected = SqliteType.Text;

        // Act
        SqliteType actual = SqliteTypeHelper.ToSqliteType(stringableType);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ToSqliteType_ThrowsNotSupportedException_WithUnsupportedTypes()
    {
        // Arrange
        Type unsupported = typeof(Convert);

        // Act
        Exception exception = Record.Exception(
            () => _ = SqliteTypeHelper.ToSqliteType(unsupported)
        );

        // Assert
        Assert.IsType<NotSupportedException>(exception);
    }
}
