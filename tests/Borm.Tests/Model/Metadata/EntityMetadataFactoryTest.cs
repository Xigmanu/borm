using Borm.Model;
using Borm.Model.Conversion;
using Borm.Model.Metadata;
using Borm.Reflection;
using Borm.Tests.Mocks;
using Moq;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityMetadataFactoryTest
{
    [Fact]
    public void Create_ReturnsEntityMetadata_WithEntityInfo()
    {
        // Arrange
        IEntityMetadata expected = EntityMetadataMockFactory.CreateMockAddressEntity();
        List<MappingMember> properties =
        [
            new(
                "Id",
                new NullableType(typeof(int), false),
                new MappingInfo(0, "id", true, false, null, ReferentialAction.NoAction), null),
            new(
                "Address",
                new NullableType(typeof(string), false),
                new MappingInfo(1, "address", false, false, null, ReferentialAction.NoAction), null),
            new(
                "Address_1",
                new NullableType(typeof(string), true),
                new MappingInfo(2, "address_1", false, false, null, ReferentialAction.NoAction), null),
            new(
                "City",
                new NullableType(typeof(string), false),
                new MappingInfo(3, "city", false, true, null, ReferentialAction.NoAction), null),
        ];

        EntityInfo typeInfo = new(expected.Name, expected.Type, properties, [], null);
        Mock<IEntityBufferConversion> conversionMock = new();

        // Act
        IEntityMetadata actual = EntityMetadataFactory.Create(
            typeInfo,
            (_, _, _) => conversionMock.Object
        );

        // Assert
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.PrimaryKey.Name, actual.PrimaryKey.Name);
        Assert.Equal(expected.Columns.Count, actual.Columns.Count);
        for (int i = 0; i < expected.Columns.Count; i++)
        {
            IColumnMetadata expectedColumn = expected.Columns[i];
            IColumnMetadata actualColumn = actual.Columns[i];

            Assert.Equal(expectedColumn.Index, actualColumn.Index);
            Assert.Equal(expectedColumn.Name, actualColumn.Name);
            Assert.Equal(expectedColumn.DataType, actualColumn.DataType);
            Assert.Equal(expectedColumn.Constraints, actualColumn.Constraints);
            Assert.Equal(expectedColumn.PropertyName, actualColumn.PropertyName);
            Assert.Equal(expectedColumn.OnDelete, actualColumn.OnDelete);
        }
    }
}
