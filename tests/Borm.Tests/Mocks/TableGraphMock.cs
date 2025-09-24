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

        graph.AddEdge(addressesTable, personsTable);
        graph.AddEdge(personsTable, employeesTable);

        return graph;
    }
}
