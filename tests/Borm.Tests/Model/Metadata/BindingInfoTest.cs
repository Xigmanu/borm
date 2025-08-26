using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class BindingInfoTest
{
    [Fact]
    public void CreateBinding_ShouldMaterialize_WithConstructorBinding()
    {
        // Arrange
        int id = 1;
        string name = "Alice";

        ColumnMetadata idColumn = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnMetadata nameColumn = new(1, "name", "Name", typeof(string), Constraints.None, null);

        ColumnMetadataCollection columns = new([idColumn, nameColumn]);

        EntityMaterializationBinding binding = new(typeof(PersonB), columns);
        EntityConversionBinding conversionBinding = binding.CreateBinding();

        ValueBuffer buffer = new();
        buffer[idColumn] = id;
        buffer[nameColumn] = name;

        // Act
        PersonB person = (PersonB)conversionBinding.MaterializeEntity(buffer);

        // Assert
        Assert.Equal(id, person.Id);
        Assert.Equal(name, person.Name);
    }

    [Fact]
    public void CreateBinding_ShouldMaterializeAndConvert_WithPropertySetter()
    {
        // Arrange
        int id = 1;
        string name = "Alice";

        ColumnMetadata idColumn = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnMetadata nameColumn = new(1, "name", "Name", typeof(string), Constraints.None, null);

        ColumnMetadataCollection columns = new([idColumn, nameColumn]);

        EntityMaterializationBinding binding = new(typeof(PersonA), columns);
        EntityConversionBinding conversionBinding = binding.CreateBinding();

        ValueBuffer buffer = new();
        buffer[idColumn] = id;
        buffer[nameColumn] = name;

        // Act
        PersonA person = (PersonA)conversionBinding.MaterializeEntity(buffer);

        // Assert
        Assert.Equal(id, person.Id);
        Assert.Equal(name, person.Name);
    }

    [Fact]
    public void CreateBinding_ShouldMaterializeEntity_WithNullableColumn()
    {
        // Arrange
        int id = 1;

        ColumnMetadata idColumn = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnMetadata nameColumn = new(
            1,
            "name",
            "Name",
            typeof(string),
            Constraints.AllowDbNull,
            null
        );

        ColumnMetadataCollection columns = new([idColumn, nameColumn]);

        EntityMaterializationBinding binding = new(typeof(PersonA), columns);
        EntityConversionBinding conversionBinding = binding.CreateBinding();

        ValueBuffer buffer = new();
        buffer[idColumn] = id;
        buffer[nameColumn] = DBNull.Value;

        // Act
        PersonA person = (PersonA)conversionBinding.MaterializeEntity(buffer);

        // Assert
        Assert.Equal(id, person.Id);
        Assert.Null(person.Name);
    }

    public sealed class PersonA
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public sealed class PersonB(int id, string name)
    {
        public int Id { get; } = id;
        public string Name { get; } = name;
    }
}
