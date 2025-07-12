namespace Borm.Tests.Schema.Metadata;

using System.Reflection;
using Borm.Schema.Metadata;
using static Borm.Tests.Mocks.ConstructorSelectorTestMocks;

public class EntityBindingInfoTest
{
    [Fact]
    public void GetOrderedColumns_ReturnsColumnsOrderedByCtorParameterList_WithValidEntityCtor()
    {
        // Arrange
        Type entityType = typeof(ValidCtorEntity);
        ConstructorInfo constructor = entityType.GetConstructors()[0];
        ColumnInfo idCol = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfo nameCol = new(1, "name", "Name", typeof(string), Constraints.None, null);
        ColumnInfoCollection columns = new([nameCol, idCol]);
        ParameterInfo[] parameters = constructor.GetParameters();

        EntityBindingInfo bindingInfo = new(entityType, columns);

        // Act
        ColumnInfo[] actualColumns = bindingInfo.GetOrderedColumns();

        // Assert
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo parameter = parameters[i];
            ColumnInfo actual = actualColumns[i];

            Assert.Equal(parameter.Name, actual.Name);
        }
    }

    [Fact]
    public void GetOrderedColumns_ReturnsColumnsOrderedByIndex_WithDefaultEntityCtor()
    {
        // Arrange
        Type entityType = typeof(DefaultCtorEntity);
        ColumnInfo idCol = new(0, "id", "Id", typeof(int), Constraints.PrimaryKey, null);
        ColumnInfo nameCol = new(1, "name", "Name", typeof(string), Constraints.None, null);
        ColumnInfoCollection columns = new([idCol, nameCol]);

        EntityBindingInfo bindingInfo = new(entityType, columns);

        // Act
        ColumnInfo[] actualColumns = bindingInfo.GetOrderedColumns();

        // Assert
        for (int i = 0; i < actualColumns.Length; i++)
        {
            ColumnInfo expected = columns.ElementAt(i);
            ColumnInfo actual = actualColumns[i];

            Assert.Equal(expected.Name, actual.Name);
        }
    }
}
