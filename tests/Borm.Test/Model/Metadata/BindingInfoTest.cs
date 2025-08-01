﻿using Borm.Model.Metadata;

namespace Borm.Tests.Model.Metadata;

public sealed class BindingInfoTest
{
    [Fact]
    public void CreateBinding_ShouldMaterialize_WithConstructorBinding()
    {
        // Arrange
        int id = 1;
        string name = "Alice";

        ColumnInfo idColumn = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfo nameColumn = new(1, "name", "Name", typeof(string), Constraints.None, null);

        ColumnInfoCollection columns = new([idColumn, nameColumn]);

        BindingInfo bindingInfo = new(typeof(PersonB), columns);
        ConversionBinding binding = bindingInfo.CreateBinding();

        ValueBuffer buffer = new();
        buffer[idColumn] = id;
        buffer[nameColumn] = name;

        // Act
        PersonB person = (PersonB)binding.MaterializeEntity(buffer);

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

        ColumnInfo idColumn = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfo nameColumn = new(1, "name", "Name", typeof(string), Constraints.None, null);

        ColumnInfoCollection columns = new([idColumn, nameColumn]);

        BindingInfo bindingInfo = new(typeof(PersonA), columns);
        ConversionBinding binding = bindingInfo.CreateBinding();

        ValueBuffer buffer = new();
        buffer[idColumn] = id;
        buffer[nameColumn] = name;

        // Act
        PersonA person = (PersonA)binding.MaterializeEntity(buffer);

        // Assert
        Assert.Equal(id, person.Id);
        Assert.Equal(name, person.Name);
    }

    [Fact]
    public void CreateBinding_ShouldMaterializeEntity_WithNullableColumn()
    {
        // Arrange
        int id = 1;

        ColumnInfo idColumn = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfo nameColumn = new(1, "name", "Name", typeof(string), Constraints.AllowDbNull, null);

        ColumnInfoCollection columns = new([idColumn, nameColumn]);

        BindingInfo bindingInfo = new(typeof(PersonA), columns);
        ConversionBinding binding = bindingInfo.CreateBinding();

        ValueBuffer buffer = new();
        buffer[idColumn] = id;
        buffer[nameColumn] = DBNull.Value;

        // Act
        PersonA person = (PersonA)binding.MaterializeEntity(buffer);

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
