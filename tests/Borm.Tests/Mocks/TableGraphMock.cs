using Borm.Data.Storage;

namespace Borm.Tests.Mocks;

internal static class TableGraphMock
{
    public static TableGraph Create()
    {
        TableGraph graph = new();

        Table addressesTable = new(EntityMetadataMockFactory.CreateMockAddressEntity());
        Table personsTable = new(EntityMetadataMockFactory.CreateMockPersonEntity());
        Table employeesTable = new(EntityMetadataMockFactory.CreateMockEmployeeEntity());

        graph.AddTable(addressesTable);
        graph.AddTable(personsTable);
        graph.AddTable(employeesTable);

        graph.AddEdge(addressesTable, personsTable);
        graph.AddEdge(personsTable, employeesTable);

        return graph;
    }
}
