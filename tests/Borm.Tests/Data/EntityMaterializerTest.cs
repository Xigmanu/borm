using Borm.Data;
using static Borm.Tests.Mocks.EntityMetadataMocks;
using static Borm.Tests.Mocks.TableMocks;
using static Borm.Tests.Mocks.ValueBufferMockHelper;

namespace Borm.Tests.Data;

public sealed class EntityMaterializerTest
{
    [Fact]
    public void Materialize_CreatesEntityObjectFromBuffer_WithRelationalEntity()
    {
        // Arrange
        Table table = PersonsTable;

        AddressEntity expectedDependency = new(1, "address", null, "city");
        Change change = Change.Initial(CreateBuffer(AddressesDummyData, AddressesTable), -1);
        AddressesTable.Tracker.PendChange(change);

        PersonEntity expected = new(1, "name", 42.619, expectedDependency);
        ValueBuffer buffer = CreateBuffer(PersonsDummyData, PersonsTable);

        EntityMaterializer materializer = new(table);

        // Act
        object actual = materializer.Materialize(buffer);

        // Assert
        Assert.IsType<PersonEntity>(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Materialize_CreatesEntityObjectFromBuffer_WithRelationalEntityAndNullDependency()
    {
        // Arrange
        Table table = PersonsTable;

        PersonEntity expected = new(1, "name", 42.619, null);
        ValueBuffer buffer = CreateBuffer(PersonsDummyData, PersonsTable);

        EntityMaterializer materializer = new(table);

        // Act
        object actual = materializer.Materialize(buffer);

        // Assert
        Assert.IsType<PersonEntity>(actual);
        Assert.Equal(expected, actual);
    }

    public void ResolveForeignKeyValues_

    [Fact]
    public void Materialize_CreatesEntityObjectFromBuffer_WithSimpleEntity()
    {
        // Arrange
        Table table = AddressesTable;
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, AddressesTable);
        AddressEntity expected = new(1, "address", null, "city");

        EntityMaterializer materializer = new(table);

        // Act
        object actual = materializer.Materialize(buffer);

        // Assert
        Assert.IsType<AddressEntity>(actual);
        Assert.Equal(expected, actual);
    }
}
