using Borm.Data;

namespace Borm.Tests.Mocks;

internal static class TableMocks
{
    public static readonly Table AddressesTable = new(EntityMetadataMocks.AddressesEntity, []);
    public static readonly Table PersonsTable = new(
        EntityMetadataMocks.PersonsEntity,
        new Dictionary<Borm.Model.Metadata.ColumnMetadata, Table>()
        {
            [EntityMetadataMocks.PersonsEntity.Columns["address"]] = AddressesTable,
        }
    );
}
