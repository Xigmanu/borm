using System.Data;
using Borm.Data;
using Borm.Model.Metadata;

namespace Borm.Tests.Data;

public sealed class EntityRepositoryTest
{
    /*
    [Fact]
    public void Delete_RemovesRow_WithSimpleEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table table = (Table)dataSet.Tables["entityA"]!;
        EntityRepository<EntityA> repository = new(table, nodeGraph);

        int id = 1;
        string value = "foo";
        string newValue = "bar";

        DataRow row = table.NewRow();
        row["id"] = id;
        row["value"] = value;
        table.Rows.Add(row);
        table.AcceptChanges();

        EntityA entity = new(id, newValue);

        // Act
        repository.Delete(entity);

        // Assert
        Assert.Single(table.Rows);
        Assert.Equal(DataRowState.Deleted, table.Rows[0].RowState);
    }

    [Fact]
    public void Delete_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        EntityRepository<EntityA> repository = new(
            (Table)dataSet.Tables["entityA"]!,
            nodeGraph
        );

        // Act
        Exception exception = Record.Exception(() => repository.Delete(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Delete_ShouldThrowRowNotFoundException_WhenRowDoesNotExist()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table table = (Table)dataSet.Tables["entityA"]!;
        EntityRepository<EntityA> repository = new(table, nodeGraph);

        int id = 1;
        string newValue = "bar";

        EntityA entity = new(id, newValue);

        // Act
        Exception exception = Record.Exception(() => repository.Delete(entity));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<RowNotFoundException>(exception);
    }

    [Fact]
    public void Insert_ShouldInsertEntityValue_WithComplexFKEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table tableA = (Table)dataSet.Tables["entityA"]!;
        tableA.Rows.Add(1, "foo");
        Table tableB = (Table)dataSet.Tables["entityB"]!;
        Table tableC = (Table)dataSet.Tables["entityC"]!;
        EntityRepository<EntityC> repository = new(tableC, nodeGraph);

        int id = 1;
        EntityB entityB = new(1, 1);
        EntityC entity = new(id, entityB);

        // Act
        repository.Insert(entity);

        // Assert
        Assert.Single(tableB.Rows);
        Assert.Single(tableC.Rows);

        DataRow row = tableC.Rows[0];
        Assert.Equal(id, row["id"]);
        Assert.Equal(entityB.Id, row["entityB"]);
    }

    [Fact]
    public void Insert_ShouldInsertEntityValue_WithSimpleEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table table = (Table)dataSet.Tables["entityA"]!;
        EntityRepository<EntityA> repository = new(table, nodeGraph);

        int id = 1;
        string value = "foo";
        EntityA entity = new(id, value);

        // Act
        repository.Insert(entity);

        // Assert
        Assert.Single(table.Rows);
        DataRow row = table.Rows[0];
        Assert.Equal(id, row["id"]);
        Assert.Equal(value, row["value"]);
    }

    [Fact]
    public void Insert_ShouldInsertEntityValue_WithSimpleFKEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table tableA = (Table)dataSet.Tables["entityA"]!;
        tableA.Rows.Add(1, "foo");
        Table tableB = (Table)dataSet.Tables["entityB"]!;
        EntityRepository<EntityB> repository = new(tableB, nodeGraph);

        int id = 1;
        int entityA = 1;
        EntityB entity = new(id, entityA);

        // Act
        repository.Insert(entity);

        // Assert
        Assert.Single(tableB.Rows);
        DataRow row = tableB.Rows[0];
        Assert.Equal(id, row["id"]);
        Assert.Equal(entityA, row["entityA"]);
    }

    [Fact]
    public void Insert_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        EntityRepository<EntityA> repository = new(
            (Table)dataSet.Tables["entityA"]!,
            nodeGraph
        );

        // Act
        Exception exception = Record.Exception(() => repository.Insert(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Insert_ShouldThrowException_WhenEntityFailsValidation()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table table = (Table)dataSet.Tables["entityA"]!;
        EntityRepository<EntityA> repository = new(table, nodeGraph);

        EntityA entity = new(1, EntityAValidator.InvalidValue);

        // Act
        Exception exception = Record.Exception(() => repository.Insert(entity));

        // Assert
        Assert.Empty(table.Rows);
        Assert.NotNull(exception);
    }

    [Fact]
    public void Insert_ShouldThrowInvalidOperationException_WhenNoDataRelationExistsBetweenTables()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table tableA = (Table)dataSet.Tables["entityA"]!;
        tableA.Rows.Add(1, "foo");
        Table tableC = (Table)dataSet.Tables["entityC"]!;
        EntityRepository<EntityC> repository = new(tableC, nodeGraph);

        dataSet.Relations.Clear();

        int id = 1;
        EntityB entityB = new(1, 1);
        EntityC entity = new(id, entityB);

        // Act
        Exception exception = Record.Exception(() => repository.Insert(entity));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Insert_ThrowConstraintException_WhenRowWithProvidedPKExists()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table table = (Table)dataSet.Tables["entityA"]!;
        EntityRepository<EntityA> repository = new(table, nodeGraph);

        int id = 1;
        string value = "foo";
        DataRow row = table.NewRow();
        row["id"] = id;
        row["value"] = value;
        table.Rows.Add(row);

        EntityA entity = new(id, value);

        // Act
        Exception exception = Record.Exception(() => repository.Insert(entity));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ConstraintException>(exception);
        Assert.Single(table.Rows);
    }

    [Fact]
    public void Select_ShouldReadRowAndMaterializeEntity_WithComplexFKEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table tableA = (Table)dataSet.Tables["entityA"]!;
        Table tableB = (Table)dataSet.Tables["entityB"]!;
        Table tableC = (Table)dataSet.Tables["entityC"]!;
        tableA.Rows.Add(1, "foo");
        tableB.Rows.Add(1, 1);
        tableC.Rows.Add(1, 1);
        EntityRepository<EntityC> repository = new(tableC, nodeGraph);

        // Act
        IEnumerable<EntityC> entities = repository.Select();

        // Assert
        Assert.Single(entities);
        EntityC entity = entities.First();
        Assert.Equal(1, entity.Id);
        Assert.Equal(1, entity.EntityB.Id);
        Assert.Equal(1, entity.EntityB.EntityA);
    }

    [Fact]
    public void Select_ShouldReadRowAndMaterializeEntity_WithSimpleEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table table = (Table)dataSet.Tables["entityA"]!;
        EntityRepository<EntityA> repository = new(table, nodeGraph);

        int id = 1;
        string value = "foo";

        DataRow row = table.NewRow();
        row["id"] = id;
        row["value"] = value;
        table.Rows.Add(row);
        table.AcceptChanges();

        // Act
        IEnumerable<EntityA> entities = repository.Select();
        IEnumerable<EntityA> cachedEntities = repository.Select();

        // Assert
        Assert.Single(entities);
        EntityA entity = entities.First();
        Assert.Equal(id, entity.Id);
        Assert.Equal(value, entity.Value);
        EntityA cached = cachedEntities.First();
        Assert.Equal(entity.Id, cached.Id);
        Assert.Equal(entity.Value, cached.Value);
    }

    [Fact]
    public void Select_ShouldReadRowAndMaterializeEntity_WithSimpleFKEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table tableA = (Table)dataSet.Tables["entityA"]!;
        Table tableB = (Table)dataSet.Tables["entityB"]!;
        tableA.Rows.Add(1, "foo");
        tableB.Rows.Add(1, 1);
        EntityRepository<EntityB> repository = new(tableB, nodeGraph);

        // Act
        IEnumerable<EntityB> entities = repository.Select();

        // Assert
        Assert.Single(entities);
        EntityB entity = entities.First();
        Assert.Equal(1, entity.Id);
        Assert.Equal(1, entity.EntityA);
    }

    [Fact]
    public void Select_ShouldThrowInvalidOperationException_WhenNoDataRelationExistsBetweenTables()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table tableA = (Table)dataSet.Tables["entityA"]!;
        Table tableB = (Table)dataSet.Tables["entityB"]!;
        Table tableC = (Table)dataSet.Tables["entityC"]!;
        tableA.Rows.Add(1, "foo");
        tableB.Rows.Add(1, 1);
        tableC.Rows.Add(1, 1);
        EntityRepository<EntityC> repository = new(tableC, nodeGraph);

        dataSet.Relations.Clear();

        // Act
        Exception exception = Record.Exception(() => _ = repository.Select());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Update_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        EntityRepository<EntityA> repository = new(
            (Table)dataSet.Tables["entityA"]!,
            nodeGraph
        );

        // Act
        Exception exception = Record.Exception(() => repository.Update(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Update_ShouldThrowRowNotFoundException_WhenRowDoesNotExist()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table table = (Table)dataSet.Tables["entityA"]!;
        EntityRepository<EntityA> repository = new(table, nodeGraph);

        int id = 1;
        string newValue = "bar";

        EntityA entity = new(id, newValue);

        // Act
        Exception exception = Record.Exception(() => repository.Update(entity));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<RowNotFoundException>(exception);
        Assert.Empty(table.Rows);
    }

    [Fact]
    public void Update_ShouldUpdateRow_WithComplexFKEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table tableA = (Table)dataSet.Tables["entityA"]!;
        Table tableB = (Table)dataSet.Tables["entityB"]!;
        Table tableC = (Table)dataSet.Tables["entityC"]!;
        tableA.Rows.Add(1, "foo");
        tableB.Rows.Add(1, 1);
        tableB.Rows.Add(2, 1);
        tableC.Rows.Add(1, 1);
        EntityRepository<EntityC> repository = new(tableC, nodeGraph);

        int id = 1;
        EntityB entityB = new(2, 1);
        EntityC entity = new(id, entityB);

        // Act
        repository.Update(entity);

        // Assert
        Assert.Single(tableC.Rows);

        DataRow row = tableC.Rows[0];
        Assert.Equal(id, row["id"]);
        Assert.Equal(entityB.Id, row["entityB"]);
    }

    [Fact]
    public void Update_ShouldUpdateRow_WithSimpleEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table table = (Table)dataSet.Tables["entityA"]!;
        EntityRepository<EntityA> repository = new(table, nodeGraph);

        int id = 1;
        string value = "foo";
        string newValue = "bar";

        DataRow row = table.NewRow();
        row["id"] = id;
        row["value"] = value;
        table.Rows.Add(row);

        EntityA entity = new(id, newValue);

        // Act
        repository.Update(entity);

        // Assert
        Assert.Single(table.Rows);
        DataRow actual = table.Rows[0];
        Assert.Equal(id, actual["id"]);
        Assert.Equal(newValue, actual["value"]);
    }

    [Fact]
    public void Update_ShouldUpdateRow_WithSimpleFKEntity()
    {
        // Arrange
        (BormDataSet dataSet, TableGraph nodeGraph) = CreateTestData();
        Table tableA = (Table)dataSet.Tables["entityA"]!;
        Table tableB = (Table)dataSet.Tables["entityB"]!;
        tableA.Rows.Add(1, "foo");
        tableA.Rows.Add(2, "bar");
        tableB.Rows.Add(1, 1);
        EntityRepository<EntityB> repository = new(tableB, nodeGraph);

        int id = 1;
        int entityA = 2;
        EntityB entity = new(id, entityA);

        // Act
        repository.Update(entity);

        // Assert
        Assert.Single(tableB.Rows);
        DataRow row = tableB.Rows[0];
        Assert.Equal(id, row["id"]);
        Assert.Equal(entityA, row["entityA"]);
    }
    */
}
