using Borm.Data.Storage;
using Borm.Model.Conversion;
using Borm.Model.Metadata;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Model.Conversion;

public sealed class ValueBufferConverterFactoryTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Create_ReturnsConverterFunction_WithValidTypeAndColumns()
    {
        // Arrange
        Type type = typeof(AddressEntity);
        IEntityMetadata metadata = _graph[type]!.Metadata;
        IReadOnlyList<IColumnMetadata> columns = metadata.Columns;

        ValueBufferConverterFactory factory = new(type, columns);

        IValueBuffer buffer = CreateBuffer(MapValuesToColumns(AddressesDummyData, columns));
        AddressEntity entity = AddressEntity.CreateFromArray(AddressesDummyData);

        // Act
        Func<object, IValueBuffer> converter = factory.Create();

        // Assert
        Assert.Equal(buffer, converter(entity));
    }
}