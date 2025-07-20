using Borm.Data;
using Borm.Model.Metadata;
using System.Data;
using static Borm.Tests.Mocks.NodeDataTableRepositoryTestMocks;

namespace Borm.Tests.Data;

// TODO Finish this
public sealed class EntityRepositoryTest
{
    [Fact]
    public void Delete_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Arrange
        (BormDataSet dataSet, EntityNodeGraph nodeGraph) = CreateTestData();
        EntityRepository<EntityA> repository = new(
            (NodeDataTable)dataSet.Tables["entityA"]!,
            nodeGraph
        );

        // Act
        Exception exception = Record.Exception(() => repository.Delete(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Insert_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Arrange
        (BormDataSet dataSet, EntityNodeGraph nodeGraph) = CreateTestData();
        EntityRepository<EntityA> repository = new(
            (NodeDataTable)dataSet.Tables["entityA"]!,
            nodeGraph
        );

        // Act
        Exception exception = Record.Exception(() => repository.Insert(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Update_ShouldThrowArgumentNullException_WhenEntityIsNull()
    {
        // Arrange
        (BormDataSet dataSet, EntityNodeGraph nodeGraph) = CreateTestData();
        EntityRepository<EntityA> repository = new(
            (NodeDataTable)dataSet.Tables["entityA"]!,
            nodeGraph
        );

        // Act
        Exception exception = Record.Exception(() => repository.Update(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<ArgumentNullException>(exception);
    }

    [Fact]
    public void Insert_ShouldInsertEntityValue_WithSimpleEntity()
    {
        // Arrange
        (BormDataSet dataSet, EntityNodeGraph nodeGraph) = CreateTestData();
        NodeDataTable table = (NodeDataTable)dataSet.Tables["entityA"]!;
        EntityRepository<EntityA> repository = new(
            table,
            nodeGraph
        );

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
        (BormDataSet dataSet, EntityNodeGraph nodeGraph) = CreateTestData();
        NodeDataTable tableA = (NodeDataTable)dataSet.Tables["entityA"]!;
        tableA.Rows.Add(1, "foo");
        NodeDataTable tableB = (NodeDataTable)dataSet.Tables["entityB"]!;
        EntityRepository<EntityB> repository = new(
            tableB,
            nodeGraph
        );
        

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
    public void Insert_ShouldInsertEntityValue_WithComplexFKEntity()
    {
        // Arrange
        (BormDataSet dataSet, EntityNodeGraph nodeGraph) = CreateTestData();
        NodeDataTable tableA = (NodeDataTable)dataSet.Tables["entityA"]!;
        tableA.Rows.Add(1, "foo");
        NodeDataTable tableB = (NodeDataTable)dataSet.Tables["entityB"]!;
        NodeDataTable tableC = (NodeDataTable)dataSet.Tables["entityC"]!;
        EntityRepository<EntityC> repository = new(
            tableC,
            nodeGraph
        );


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
}
