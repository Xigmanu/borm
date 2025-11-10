using Borm.Model;
using Borm.Reflection;

namespace Borm.Tests.Reflection;

public sealed class MappingInfoTest
{
    [Fact]
    public void FromAttribute_ReturnsMappingInfo_WithColumnAttribute()
    {
        // Arrange
        int index = 0;
        string name = "fk";
        ReferentialAction action = ReferentialAction.NoAction;

        ColumnAttribute attribute = new(index, name);

        // Act
        MappingInfo info = MappingInfo.FromAttribute(attribute);

        // Assert
        Assert.Equal(index, info.ColumnIndex);
        Assert.Equal(name, info.ColumnName);
        Assert.False(info.IsPrimaryKey);
        Assert.False(info.IsUnique);
        Assert.Null(info.Reference);
        Assert.Equal(action, info.OnDelete);
    }

    [Fact]
    public void FromAttribute_ReturnsMappingInfo_WithForeignKeyAttribute()
    {
        // Arrange
        int index = 0;
        string name = "fk";
        Type reference = typeof(int);
        ReferentialAction action = ReferentialAction.Cascade;

        ForeignKeyAttribute attribute = new(index, name, reference)
        {
            OnDelete = action,
            IsUnique = true
        };

        // Act
        MappingInfo info = MappingInfo.FromAttribute(attribute);

        // Assert
        Assert.Equal(index, info.ColumnIndex);
        Assert.Equal(name, info.ColumnName);
        Assert.False(info.IsPrimaryKey);
        Assert.True(info.IsUnique);
        Assert.Equal(reference, info.Reference);
        Assert.Equal(action, info.OnDelete);
    }

    [Fact]
    public void FromAttribute_ReturnsMappingInfo_WithPrimaryKeyAttribute()
    {
        // Arrange
        int index = 0;
        string name = "fk";
        ReferentialAction action = ReferentialAction.NoAction;

        PrimaryKeyAttribute attribute = new(index, name);

        // Act
        MappingInfo info = MappingInfo.FromAttribute(attribute);

        // Assert
        Assert.Equal(index, info.ColumnIndex);
        Assert.Equal(name, info.ColumnName);
        Assert.True(info.IsPrimaryKey);
        Assert.False(info.IsUnique);
        Assert.Null(info.Reference);
        Assert.Equal(action, info.OnDelete);
    }
}