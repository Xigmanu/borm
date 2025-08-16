using Borm.Model;
using Borm.Model.Metadata;
using Borm.Reflection;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityMetadataFactoryTest
{
    [Fact]
    public void Build_ReturnsNewEntityNode_WithReflectedInformation()
    {
        // Arrange
        Type type = typeof(EntityMetadataFactoryTest);
        EntityAttribute attribute = new("foo");

        Property pKColumn = new("id", new PrimaryKeyAttribute(0), false, typeof(int));
        Property columnUsrName = new(
            "V",
            new ColumnAttribute(1, "value0"),
            false,
            typeof(string)
        );
        Property columnAutoName = new("Value1", new ColumnAttribute(2), true, typeof(string));
        Property fkColumn = new(
            "FkColumn",
            new ForeignKeyAttribute(3, typeof(decimal)),
            true,
            typeof(int)
        );

        Property[] columns = [pKColumn, columnUsrName, columnAutoName, fkColumn];
        ReflectedTypeInfo reflectedInfo = new(type, attribute, columns);

        // Act
        EntityMetadata info = EntityMetadataBuilder.Build(reflectedInfo);

        // Assert

        Assert.Equal(attribute.Name, info.Name);
        Assert.Equal(type, info.DataType);
        Assert.Equal(columns.Length, info.Columns.Count);
        for (int i = 0; i < columns.Length; i++)
        {
            Property column = columns[i];
            ColumnMetadata actual = info.Columns.ElementAt(i);

            Assert.Equal(column.Name, actual.PropertyName);
            Assert.Equal(column.Type, actual.PropertyType);
            Assert.Equal(column.Attribute.Index, actual.Index);

            string? expectedName = column.Attribute.Name;
            if (string.IsNullOrEmpty(expectedName))
            {
                expectedName = char.ToLower(column.Name[0]) + column.Name[1..];
            }
            Assert.Equal(expectedName, actual.Name);

            bool isNullable = actual.Constraints.HasFlag(Constraints.AllowDbNull);
            if (column.IsNullable)
            {
                Assert.True(isNullable);
            }
            else
            {
                Assert.False(isNullable);
            }

            bool isPk = actual.Constraints.HasFlag(Constraints.PrimaryKey);
            if (column.Attribute is PrimaryKeyAttribute)
            {
                Assert.True(isPk);
            }
            else
            {
                Assert.False(isPk);
            }

            if (column.Attribute is ForeignKeyAttribute)
            {
                Assert.NotNull(actual.Reference);
            }
            else
            {
                Assert.Null(actual.Reference);
            }
        }
    }
}
