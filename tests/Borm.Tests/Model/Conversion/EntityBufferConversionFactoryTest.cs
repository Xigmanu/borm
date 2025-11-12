using Borm.Model.Conversion;
using Borm.Model.Metadata;
using Borm.Properties;
using Borm.Reflection;
using Borm.Tests.Common;
using Borm.Tests.Mocks;

namespace Borm.Tests.Model.Conversion;

public sealed class EntityBufferConversionFactoryTest
{
    [Fact]
    public void Create_ThrowsMissingMethodException_WhenNoConstructorsAreProvided()
    {
        // Arrange
        Type entityType = typeof(AddressEntity);
        IEntityMetadata entityMetadata = EntityMetadataMockFactory.CreateMockAddressEntity();
        IReadOnlyList<IColumnMetadata> columns = entityMetadata.Columns;

        IReadOnlyList<Constructor> constructors = [];

        // Act
        Exception? exception = Record.Exception(
            () => _ = EntityBufferConversionFactory.Create(entityType, constructors, columns)
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<MissingMethodException>(exception);
        Assert.Equal(
            Strings.InvalidEntityTypeConstructor(entityType.FullName ?? entityType.Name),
            exception.Message
        );
    }
}
