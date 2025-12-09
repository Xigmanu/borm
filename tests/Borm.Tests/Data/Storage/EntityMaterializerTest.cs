using Borm.Data.Storage;
using Borm.Data.Storage.Tracking;
using Borm.Tests.Common;
using Borm.Tests.Mocks;
using static Borm.Tests.Mocks.ValueBufferMockFactory;

namespace Borm.Tests.Data.Storage;

public sealed class EntityMaterializerTest
{
    private readonly TableGraph _graph = TableGraphMock.Create();

    [Fact]
    public void Materialize_CreatesEntityObjectFromBuffer_WithRelationalEntity()
    {
        // Arrange
        ITable addressesTable = _graph[typeof(AddressEntity)]!;
        ITable personsTable = _graph[typeof(PersonEntity)]!;

        AddressEntity expectedDependency = new(1, "address", null, "city");
        IChange change = ChangeFactory.Initial(
            CreateBuffer(MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)),
            -1
        );
        addressesTable.Tracker.PendChange(change);

        PersonEntity expected = new(1, "name", 42.619, expectedDependency);
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(PersonsDummyData, personsTable.Metadata.Columns)
        );

        EntityMaterializer materializer = new(_graph);

        // Act
        object actual = materializer.Materialize(buffer, personsTable);

        // Assert
        Assert.IsType<PersonEntity>(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Materialize_CreatesEntityObjectFromBuffer_WithRelationalEntityAndNullDependency()
    {
        // Arrange
        ITable personsTable = _graph[typeof(PersonEntity)]!;

        PersonEntity expected = new(1, "name", 42.619, null);
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(PersonsDummyData, personsTable.Metadata.Columns)
        );

        EntityMaterializer materializer = new(_graph);

        // Act
        object actual = materializer.Materialize(buffer, personsTable);

        // Assert
        Assert.IsType<PersonEntity>(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Materialize_CreatesEntityObjectFromBuffer_WithSimpleEntity()
    {
        // Arrange
        ITable addressesTable = _graph[typeof(AddressEntity)]!;
        IValueBuffer buffer = CreateBuffer(
            MapValuesToColumns(AddressesDummyData, addressesTable.Metadata.Columns)
        );
        AddressEntity expected = new(1, "address", null, "city");

        EntityMaterializer materializer = new(_graph);

        // Act
        object actual = materializer.Materialize(buffer, addressesTable);

        // Assert
        Assert.IsType<AddressEntity>(actual);
        Assert.Equal(expected, actual);
    }
}