using Borm.Data;

namespace Borm.Tests.Data;

public sealed class ObjectCacheTest
{
    [Fact]
    public void Add_ShouldMapPrimaryKeyValueToEntity_WithPrimaryKeyAndEntityObject()
    {
        // Arrange
        object primaryKey = 1;
        object entity = "foo";

        ObjectCache cache = new();

        // Act
        cache.Add(primaryKey, entity);

        // Assert
        Assert.Single(cache.Values);
        object? actual = cache.Find(primaryKey);
        Assert.NotNull(actual);
        Assert.Equal(entity, actual);
    }

    [Fact]
    public void Find_ReturnsEntityObject_WhenPrimaryKeyExists()
    {
        // Arrange
        object primaryKey = 1;
        object entity = "foo";

        ObjectCache cache = new();
        cache.Add(primaryKey, entity);

        // Act
        object? actual = cache.Find(primaryKey);

        // Assert
        Assert.NotNull(actual);
    }

    [Fact]
    public void Find_ReturnsNull_WhenPrimaryKeyDoesNotExist()
    {
        // Arrange
        object primaryKey = 1;
        object entity = "foo";

        ObjectCache cache = new();
        cache.Add(primaryKey, entity);

        // Act
        object? actual = cache.Find(2);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void Remove_ShouldRemoveEntityByItsPrimaryKey_WithPrimaryKey()
    {
        // Arrange
        object primaryKey = 1;
        object entity = "foo";

        ObjectCache cache = new();
        cache.Add(primaryKey, entity);

        // Act
        cache.Remove(primaryKey);

        // Assert
        Assert.Empty(cache.Values);
    }

    [Fact]
    public void Update_ShouldOverrideEntityObject_WithPrimaryKeyAndNewEntityObject()
    {
        // Arrange
        object primaryKey = 1;
        object entity = "foo";
        object newEntity = "bar";

        ObjectCache cache = new();
        cache.Add(primaryKey, entity);

        // Act
        cache.Update(primaryKey, newEntity);

        // Assert
        Assert.Single(cache.Values);
        object? actual = cache.Find(primaryKey);
        Assert.NotNull(actual);
        Assert.Equal(newEntity, actual);
    }
}
