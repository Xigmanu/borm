using System.Linq.Expressions;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Tests.Model.Metadata;

public sealed class ConstructorSelectorTest
{
    [Fact]
    public void FindMappingCtor_ReturnsConstructor_WhenValidConstructorExists()
    {
        // Arrange
        const string mappingName = "foo";
        MappingMember parameter = new(mappingName, new NullableType(typeof(int), false), null, null);
        Constructor constructor = new(false, [parameter], _ => Expression.Empty());
        HashSet<string> columnNames = [mappingName];

        // Act
        Constructor? actual = ConstructorSelector.FindMappingCtor([constructor], columnNames);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(constructor, actual);
    }

    [Fact]
    public void FindMappingCtor_ReturnsDefaultConstructor_WhenCollectionContainsOnlyImplicitConstructor()
    {
        // Arrange
        Constructor constructor = new(true, [], _ => Expression.Empty());
        HashSet<string> columnNames = ["foo"];

        // Act
        Constructor? actual = ConstructorSelector.FindMappingCtor([constructor], columnNames);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.IsDefault);
    }

    [Fact]
    public void FindMappingCtor_ReturnsNull_WhenNoMappingConstructorWasFound()
    {
        // Arrange
        MappingMember parameter = new("bar", new NullableType(typeof(int), false), null, null);
        Constructor constructor = new(false, [parameter], _ => Expression.Empty());
        HashSet<string> columnNames = ["foo"];

        // Act
        Constructor? actual = ConstructorSelector.FindMappingCtor([constructor], columnNames);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void FindMappingCtor_ReturnsNull_WhenNoMappingParameterAndColumnCountsMismatch()
    {
        // Arrange
        Constructor constructor = new(false, [], _ => Expression.Empty());
        HashSet<string> columnNames = ["foo"];

        // Act
        Constructor? actual = ConstructorSelector.FindMappingCtor([constructor], columnNames);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void FindMappingCtor_ThrowsArgumentException_WhenColumnsAreEmpty()
    {
        // Arrange
        Constructor constructor = new(true, [], _ => Expression.Empty());
        HashSet<string> columnNames = [];

        // Act
        Exception? exception = Record.Exception(
            () => _ = ConstructorSelector.FindMappingCtor([constructor], columnNames)
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }
}
