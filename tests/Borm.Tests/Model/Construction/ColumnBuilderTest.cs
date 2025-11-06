using Borm.Model;
using Borm.Model.Construction;
using Borm.Model.Validators;
using Borm.Reflection;
using Borm.Tests.Common;

namespace Borm.Tests.Model.Construction;

public sealed class ColumnBuilderTest
{
    private static readonly IValidator<ColumnBuilder<AddressEntity>.Configuration> TestAddressValidator =
        new TestColumnBuilderConfigurationValidator<AddressEntity>();
    private static readonly IValidator<ColumnBuilder<PersonEntity>.Configuration> TestPersonValidator =
        new TestColumnBuilderConfigurationValidator<PersonEntity>();

    [Fact]
    public void Build_Column_WithValidConfiguration()
    {
        // Arrange
        int idx = 0;
        string name = "id";
        string memberName = "Id";
        Type type = typeof(int);

        ColumnBuilder<AddressEntity> builder = new(TestAddressValidator);

        // Act
        MappingMember column = builder
            .Index(idx)
            .Mapping((e) => e.Id, name)
            .PrimaryKey()
            .Unique()
            .Build();

        // Assert
        Assert.Equal(memberName, column.MemberName);
        Assert.Equal(type, column.Type.Type);
        Assert.NotNull(column.Mapping);
        Assert.Equal(idx, column.Mapping.ColumnIndex);
        Assert.Equal(name, column.Mapping.ColumnName);
        Assert.True(column.Mapping.IsPrimaryKey);
        Assert.True(column.Mapping.IsUnique);
        Assert.Null(column.Mapping.Reference);
    }

    [Fact]
    public void Build_ForeignKeyColumn_WithValidConfiguration()
    {
        // Arrange
        int idx = 0;
        string name = "address";
        string memberName = "Address";
        Type type = typeof(AddressEntity);
        ReferentialAction action = ReferentialAction.Cascade;

        ColumnBuilder<PersonEntity> builder = new(TestPersonValidator);

        // Act
        MappingMember column = builder
            .Index(idx)
            .Mapping((e) => e.Address, name)
            .References(type)
            .OnDelete(action)
            .Build();

        // Assert
        Assert.Equal(memberName, column.MemberName);
        Assert.Equal(type, column.Type.Type);
        Assert.NotNull(column.Mapping);
        Assert.Equal(idx, column.Mapping.ColumnIndex);
        Assert.Equal(name, column.Mapping.ColumnName);
        Assert.False(column.Mapping.IsPrimaryKey);
        Assert.False(column.Mapping.IsUnique);
        Assert.Equal(type, column.Mapping.Reference);
        Assert.Equal(action, column.Mapping.OnDelete);
    }

    [Fact]
    public void Index_ThrowsArgumentException_WithInvalidIndex()
    {
        // Arrange
        int index = -1;

        ColumnBuilder<AddressEntity> builder = new(TestAddressValidator);

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Index(index));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Mapping_ThrowsArgumentException_WhenExpressionIsNotMemberExpression()
    {
        // Arrange
        ColumnBuilder<AddressEntity> builder = new(TestAddressValidator);

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Mapping(e => e.Id + 42));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Mapping_ThrowsArgumentException_WithInvalidColumnName(string name)
    {
        // Arrange
        ColumnBuilder<AddressEntity> builder = new(TestAddressValidator);

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Mapping((e) => e.Address, name));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    [Fact]
    public void Mapping_ThrowsArgumentException_WithNullColumnName()
    {
        // Arrange
        ColumnBuilder<AddressEntity> builder = new(TestAddressValidator);

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Mapping(e => e.Address, null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Mapping_ThrowsArgumentException_WithNullMemberName()
    {
        // Arrange
        ColumnBuilder<AddressEntity> builder = new(TestAddressValidator);

        // Act
        Exception? exception = Record.Exception(() => _ = builder.Mapping<string>(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void References_ThrowsArgumentException_WhenReferencingSelf()
    {
        // Arrange
        ColumnBuilder<AddressEntity> builder = new(TestAddressValidator);

        // Act
        Exception? exception = Record.Exception(
            () => _ = builder.References(typeof(AddressEntity))
        );

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentException>(exception);
    }

    private sealed class TestColumnBuilderConfigurationValidator<TEntity>
        : IValidator<ColumnBuilder<TEntity>.Configuration>
        where TEntity : class
    {
        public void Validate(ColumnBuilder<TEntity>.Configuration value) { }
    }
}
