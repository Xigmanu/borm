using Borm.Data.Storage;
using Borm.Model.Conversion;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Model.Conversion;

public sealed class PropertyConverterFactoryTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenColumnsAreEmpty()
    {
        // Arrange
        IEnumerable<IColumnMetadata> columns = [];
        Type type = typeof(AddressEntity);

        // Act
        Exception? exception = Record.Exception(
            () => _ = new PropertyConverterFactory(type, columns)
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Create_ReturnsConversionFunction_WithValidEntityTypeAndColumns()
    {
        // Arrange
        Type type = typeof(EmployeeEntity);
        IEntityMetadata metadata = _graph[type]!.Metadata;
        IReadOnlyList<IColumnMetadata> columns = metadata.Columns;

        PropertyConverterFactory factory = new(type, columns);

        IValueBuffer buffer = CreateBuffer(MapValuesToColumns(EmployeesDummyData, columns));
        EmployeeEntity entity = EmployeeEntity.CreateFromArray(EmployeesDummyData);

        // Act
        Func<IValueBuffer, object> converter = factory.Create();

        // Assert
        Assert.Equal(entity, converter(buffer));
    }
}
