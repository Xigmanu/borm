using Borm.Data.Storage;

namespace Borm.Tests.Mocks;

internal static class TableMocks
{
    public static Table CreateAddressesTable() => new(EntityMetadataMocks.AddressesEntity);

    public static Table CreateEmployeesTable(Table personsTable)
    {
        Table table = new(EntityMetadataMocks.EmployeesEntity);
        table.ParentRelations[EntityMetadataMocks.EmployeesEntity.Columns["person_id"]] =
            personsTable;
        return table;
    }

    public static Table CreatePersonsTable(Table addressesTable)
    {
        Table table = new(EntityMetadataMocks.PersonsEntity);
        table.ParentRelations[EntityMetadataMocks.PersonsEntity.Columns["address"]] =
            addressesTable;
        return table;
    }
}
