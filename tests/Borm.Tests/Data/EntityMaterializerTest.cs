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
        Table addressesTable = CreateAddressesTable();
        Table personsTable = CreatePersonsTable(addressesTable);

        AddressEntity expectedDependency = new(1, "address", null, "city");
        Change change = Change.Initial(CreateBuffer(AddressesDummyData, addressesTable), -1);
        addressesTable.Tracker.PendChange(change);

        PersonEntity expected = new(1, "name", 42.619, expectedDependency);
        ValueBuffer buffer = CreateBuffer(PersonsDummyData, personsTable);

        EntityMaterializer materializer = new(personsTable);

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
        Table personsTable = CreatePersonsTable(CreateAddressesTable());

        PersonEntity expected = new(1, "name", 42.619, null);
        ValueBuffer buffer = CreateBuffer(PersonsDummyData, personsTable);

        EntityMaterializer materializer = new(personsTable);

        // Act
        object actual = materializer.Materialize(buffer);

        // Assert
        Assert.IsType<PersonEntity>(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Materialize_CreatesEntityObjectFromBuffer_WithSimpleEntity()
    {
        // Arrange
        Table table = CreateAddressesTable();
        ValueBuffer buffer = CreateBuffer(AddressesDummyData, table);
        AddressEntity expected = new(1, "address", null, "city");

        EntityMaterializer materializer = new(table);

        // Act
        object actual = materializer.Materialize(buffer);

        // Assert
        Assert.IsType<AddressEntity>(actual);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ResolveForeignKeys_ReturnsResolvedForeignKeyValues_WithComplexRelationalEntity()
    {
        // Arrange
        long initTxId = -1;
        Table addressesTable = CreateAddressesTable();
        Table personsTable = CreatePersonsTable(addressesTable);

        AddressEntity address = new(1, "address", null, "city");
        Change change = Change.NewChange(
            CreateBuffer(AddressesDummyData, addressesTable),
            initTxId
        );
        addressesTable.Tracker.PendChange(change);
        addressesTable.AcceptPendingChanges(initTxId);

        ValueBuffer buffer = CreateBuffer([1, "name", 42.619, address], personsTable);

        EntityMaterializer materializer = new(personsTable);

        // Act
        IEnumerable<EntityMaterializer.ResolvedForeignKey> resolvedKeys =
            materializer.ResolveForeignKeys(buffer, 0);

        // Assert
        Assert.Single(resolvedKeys);
        EntityMaterializer.ResolvedForeignKey key = resolvedKeys.First();

        Assert.Equal(personsTable.EntityMetadata.Columns["address"], key.Column);
        Assert.Equal(buffer["address"], key.RawValue);
        Assert.Equal(address.Id, key.ResolvedKey);
        Assert.True(key.ChangeExists);
    }

    [Fact]
    public void ResolveForeignKeys_ReturnsResolvedForeignKeyValues_WithSimpleRelationalEntity()
    {
        // Arrange
        long initTxId = -1;
        Table addressesTable = CreateAddressesTable();
        Table personsTable = CreatePersonsTable(addressesTable);
        Table employeesTable = CreateEmployeesTable(personsTable);

        Change change0 = Change.NewChange(
            CreateBuffer(AddressesDummyData, addressesTable),
            initTxId
        );
        addressesTable.Tracker.PendChange(change0);
        addressesTable.AcceptPendingChanges(initTxId);

        Change change1 = Change.NewChange(CreateBuffer(PersonsDummyData, personsTable), initTxId);
        personsTable.Tracker.PendChange(change1);
        personsTable.AcceptPendingChanges(initTxId);

        ValueBuffer buffer = CreateBuffer(EmployeesDummyData, employeesTable);

        EntityMaterializer materializer = new(employeesTable);

        // Act
        IEnumerable<EntityMaterializer.ResolvedForeignKey> resolvedKeys =
            materializer.ResolveForeignKeys(buffer, 0);

        // Assert
        Assert.Single(resolvedKeys);
        EntityMaterializer.ResolvedForeignKey key = resolvedKeys.First();
        Assert.Equal(key.RawValue, key.ResolvedKey);
        Assert.Equal(1, key.RawValue);
        Assert.True(key.ChangeExists);
    }

    [Fact]
    public void ResolveForeignKeys_ThrowsRowNotFoundException_WhenDependencyChangeNotExists()
    {
        // Arrange
        Table addressesTable = CreateAddressesTable();
        Table personsTable = CreatePersonsTable(addressesTable);
        Table employeesTable = CreateEmployeesTable(personsTable);

        ValueBuffer buffer = CreateBuffer(EmployeesDummyData, employeesTable);

        EntityMaterializer materializer = new(employeesTable);

        // Act
        Exception? exception = Record.Exception(
            () => _ = materializer.ResolveForeignKeys(buffer, 0)
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<RowNotFoundException>(exception);
    }
}
