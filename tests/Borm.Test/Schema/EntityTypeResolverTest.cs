using Borm.Schema;
using static Borm.Tests.Mocks.EntityTypeResolverTestMocks;

namespace Borm.Tests.Schema;

public class EntityTypeResolverTest
{
    [Fact]
    public void GetTypes_ReturnsEnumerationOfEntityTypes_WithEnumerationOfTypes()
    {
        // Arrange
        IEnumerable<Type> types = [typeof(int), typeof(int), typeof(MockEntity)];

        // Act
        IEnumerable<Type> entityTypes = EntityTypeResolver.GetTypes(types);

        // Assert
        Assert.Single(entityTypes);
        Assert.Equal(typeof(MockEntity), entityTypes.First());
    }

    [Fact]
    public void GetTypes_ThrowsArgumentException_WhenEntityTypeIsAnAbstractClass()
    {
        // Arrange
        IEnumerable<Type> types = [typeof(int), typeof(int), typeof(AbstractMockEntity)];

        // Act
        Exception exception = Record.Exception(() => _ = EntityTypeResolver.GetTypes(types));

        // Assert
        Assert.IsType<ArgumentException>(exception);
    }
}
