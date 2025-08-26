using Borm.Data.Storage;

namespace Borm.Tests.Mocks;

internal static class TableMocks
{
    public static Table CreateAddressesTable() => new(EntityMetadataMocks.AddressesEntity, []);

    public static Table CreateEmployeesTable(Table personsTable) =>
        new(
            EntityMetadataMocks.EmployeesEntity,
            new Dictionary<Borm.Model.Metadata.ColumnMetadata, Table>()
            {
                [EntityMetadataMocks.EmployeesEntity.Columns["person_id"]] = personsTable,
            }
        );

    public static Table CreatePersonsTable(Table addressesTable) =>
        new(
            EntityMetadataMocks.PersonsEntity,
            new Dictionary<Borm.Model.Metadata.ColumnMetadata, Table>()
            {
                [EntityMetadataMocks.PersonsEntity.Columns["address"]] = addressesTable,
            }
        );
}
