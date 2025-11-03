using System.Linq.Expressions;
using Borm.Data.Storage;
using Borm.Model.Conversion;
using Borm.Model.Metadata;
using Borm.Reflection;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Model.Conversion;

public sealed class ConstructorConverterFactoryTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenColumnEnumerationIsEmpty()
    {
        // Arrange
        Constructor constructor = new(
            isDefault: true,
            [],
            e => throw new NotImplementedException()
        );
        IEnumerable<IColumnMetadata> columns = [];

        // Act
        Exception? exception = Record.Exception(
            () => _ = new ConstructorConverterFactory(constructor, columns)
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Constructor_ThrowsArgumentException_WhenConstructorIsDefault()
    {
        // Arrange
        Constructor constructor = new(
            isDefault: true,
            [],
            e => throw new NotImplementedException()
        );
        IEnumerable<IColumnMetadata> columns = _graph[typeof(AddressEntity)]!.Metadata.Columns;

        // Act
        Exception? exception = Record.Exception(
            () => _ = new ConstructorConverterFactory(constructor, columns)
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Create_ReturnsConversionFunction_WithValidConstructorAndColumns()
    {
        // Arrange
        List<MappingMember> ctorParams =
        [
            new MappingMember("id", new NullableType(typeof(int), isNullable: false), null),
            new MappingMember("address", new NullableType(typeof(string), isNullable: false), null),
            new MappingMember(
                "address_1",
                new NullableType(typeof(string), isNullable: true),
                null
            ),
            new MappingMember("city", new NullableType(typeof(string), isNullable: false), null),
        ];
        Type type = typeof(AddressEntity);
        Constructor constructor = new(
            isDefault: false,
            ctorParams,
            args => Expression.New(type.GetConstructors()[0], args)
        );
        IEntityMetadata metadata = _graph[type]!.Metadata;
        IReadOnlyList<IColumnMetadata> columns = metadata.Columns;

        ConstructorConverterFactory factory = new(constructor, columns);

        IValueBuffer buffer = CreateBuffer(MapValuesToColumns(AddressesDummyData, columns));
        AddressEntity entity = AddressEntity.CreateFromArray(AddressesDummyData);

        // Act
        Func<IValueBuffer, object> converter = factory.Create();

        // Assert
        Assert.Equal(entity, converter(buffer));
    }
}
