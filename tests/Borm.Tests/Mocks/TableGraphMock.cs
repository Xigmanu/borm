using Borm.Data.Storage;
using Borm.Data.Storage.Internal;

namespace Borm.Tests.Mocks;

internal static class TableGraphMock
{
    public static ITableGraph Create()
    {
        TableGraph graph = new();

        ITable addressesTable = new Table(EntityMetadataMockFactory.CreateMockAddressEntity());
        ITable personsTable = new Table(EntityMetadataMockFactory.CreateMockPersonEntity());
        ITable employeesTable = new Table(EntityMetadataMockFactory.CreateMockEmployeeEntity());

        graph.AddTable(addressesTable);
        graph.AddTable(personsTable);
        graph.AddTable(employeesTable);

        graph.AddEdge(addressesTable, personsTable);
        graph.AddEdge(personsTable, employeesTable);

        return graph;
    }
}