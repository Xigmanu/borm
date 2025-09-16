using Borm.Data.Storage;

namespace Borm.Tests.Mocks;

internal static class TableGraphMock
{
    public static TableGraph Create()
    {
        TableGraph graph = new();

        Table addressesTable = new(EntityMetadataMocks.AddressesMetadata);
        Table personsTable = new(EntityMetadataMocks.PersonsMetadata);
        Table employeesTable = new(EntityMetadataMocks.EmployeesMetadata);

        graph.AddTable(addressesTable);
        graph.AddTable(personsTable);
        graph.AddTable(employeesTable);

        graph.AddChild(addressesTable, personsTable);
        graph.AddParent(personsTable, addressesTable);

        graph.AddChild(personsTable, employeesTable);
        graph.AddParent(employeesTable, personsTable);

        return graph;
    }
}
