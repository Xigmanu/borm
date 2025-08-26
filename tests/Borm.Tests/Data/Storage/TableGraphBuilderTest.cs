using Borm.Data.Storage;
using Borm.Model.Metadata;
using static Borm.Tests.Mocks.EntityMetadataMocks;
using static Borm.Tests.Mocks.TableMocks;

namespace Borm.Tests.Data.Storage;

public sealed class TableGraphBuilderTest
{
    [Fact]
    public void BuildAll_ReturnsRangeOfTables_WithEntityMetadataRange()
    {
        // Arrange
        Table addressesTable = CreateAddressesTable();
        Table personsTable = CreatePersonsTable(addressesTable);
        List<EntityMetadata> metadata = [AddressesEntity, PersonsEntity];
        List<Table> expected = [addressesTable, personsTable];

        TableGraphBuilder builder = new(metadata);

        // Act
        IEnumerable<Table> tables = builder.BuildAll();

        // Assert
        Assert.Equal(expected.Count, tables.Count());
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], tables.ElementAt(i));
        }
    }

    [Fact]
    public void BuildAll_ThrowsInvalidOperationException_WhenReferencePointsToNonExistentNodeType()
    {
        // Arrange
        List<EntityMetadata> metadata = [PersonsEntity];

        TableGraphBuilder builder = new(metadata);

        // Act
        Exception? exception = Record.Exception(() => _ = builder.BuildAll());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }
}
