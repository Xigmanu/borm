namespace Borm.Tests;

public class TypeMismatchExceptionTest
{
    [Fact]
    public void Constructor_SetsActualAndExpectedTypes_WithCtorArgs()
    {
        // Arrange
        Type expected = typeof(int);
        Type actual = typeof(float);

        // Act
        TypeMismatchException exception = new(string.Empty, expected, actual);

        // Assert
        Assert.Equal(expected, exception.Expected);
        Assert.Equal(actual, exception.Actual);
    }
}
