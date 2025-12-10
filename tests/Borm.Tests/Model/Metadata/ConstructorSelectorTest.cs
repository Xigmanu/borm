using Borm.Model.Metadata.Internal;
using Borm.Reflection;
using Borm.Tests.Mocks;

namespace Borm.Tests.Model.Metadata;

public sealed class ConstructorSelectorTest
{
    [Fact]
    public void FindMappingCtor_ReturnsConstructor_WhenValidConstructorExists()
    {
        // Arrange
        const string mappingName = "foo";
        IMappable parameter = new TestParameter(mappingName, new NullableType(typeof(int), false));
        IConstructor constructor = new TestConstructor(false, [parameter]);
        HashSet<string> columnNames = [mappingName];

        // Act
        IConstructor? actual = ConstructorSelector.FindMappingCtor([constructor], columnNames);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(constructor, actual);
    }

    [Fact]
    public void FindMappingCtor_ReturnsDefaultConstructor_WhenCollectionContainsOnlyImplicitConstructor()
    {
        // Arrange
        IConstructor constructor = new TestConstructor(true, []);
        HashSet<string> columnNames = ["foo"];

        // Act
        IConstructor? actual = ConstructorSelector.FindMappingCtor([constructor], columnNames);

        // Assert
        Assert.NotNull(actual);
        Assert.True(actual.IsDefault);
    }

    [Fact]
    public void FindMappingCtor_ReturnsNull_WhenNoMappingConstructorWasFound()
    {
        // Arrange
        IMappable parameter = new TestParameter("bar", new NullableType(typeof(int), false));
        IConstructor constructor = new TestConstructor(false, [parameter]);
        HashSet<string> columnNames = ["foo"];

        // Act
        IConstructor? actual = ConstructorSelector.FindMappingCtor([constructor], columnNames);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void FindMappingCtor_ReturnsNull_WhenNoMappingParameterAndColumnCountsMismatch()
    {
        // Arrange
        IConstructor constructor = new TestConstructor(false, []);
        HashSet<string> columnNames = ["foo"];

        // Act
        IConstructor? actual = ConstructorSelector.FindMappingCtor([constructor], columnNames);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void FindMappingCtor_ThrowsArgumentException_WhenColumnsAreEmpty()
    {
        // Arrange
        IConstructor constructor = new TestConstructor(true, []);
        HashSet<string> columnNames = [];

        // Act
        Exception? exception =
            Record.Exception(() => _ = ConstructorSelector.FindMappingCtor([constructor], columnNames)
            );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }
}