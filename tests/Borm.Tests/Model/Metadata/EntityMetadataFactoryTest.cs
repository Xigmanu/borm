using Borm.Data.Storage;
using Borm.Model;
using Borm.Model.Conversion;
using Borm.Model.Metadata;
using Borm.Model.Metadata.Internal;
using Borm.Reflection;

namespace Borm.Tests.Model.Metadata;

public sealed class EntityMetadataFactoryTest
{
    [Fact]
    public void Create_BuildsMetadata_WithDefaultName()
    {
        // Arrange
        EntityMetadataFactory factory = new(
            new TestEntityBufferConversionFactory(),
            new TestColumnMetadataFactory()
        );

        TestMappable property = new(
            "foo",
            new NullableType(typeof(int), false),
            new MappingInfo(0, "bar", false, false, null, ReferentialAction.NoAction),
            null
        );
        EntityInfo entity = new(null, typeof(EntityMetadataFactoryTest), [property], [], null);

        // Act
        IEntityMetadata metadata = factory.Create(entity);

        // Assert
        Assert.Equal("entityMetadataFactoryTest", metadata.Name);
        Assert.Single(metadata.Columns);
        Assert.Equal(typeof(EntityMetadataFactoryTest), metadata.Type);
    }

    private sealed class TestColumnMetadata : IColumnMetadata
    {
        public TestColumnMetadata(
            string name,
            Constraints constraints,
            NullableType dataType,
            int index,
            ReferentialAction onDelete,
            string propertyName,
            Type? reference
        )
        {
            Name = name;
            Constraints = constraints;
            DataType = dataType;
            Index = index;
            OnDelete = onDelete;
            PropertyName = propertyName;
            Reference = reference;
        }

        public Constraints Constraints { get; }
        public NullableType DataType { get; }
        public int Index { get; }
        public string Name { get; }
        public ReferentialAction OnDelete { get; }
        public string PropertyName { get; }
        public Type? Reference { get; }
    }

    private sealed class TestColumnMetadataFactory : MetadataFactory<IMappable, IColumnMetadata>
    {
        public override IColumnMetadata Create(IMappable source)
        {
            return new TestColumnMetadata(
                source.MemberName,
                Constraints.None,
                source.DataType,
                source.Mapping!.ColumnIndex,
                ReferentialAction.NoAction,
                source.MemberName,
                null
            );
        }
    }

    private sealed class TestEntityBufferConversion : IEntityBufferConversion
    {
        public object MaterializeEntity(IValueBuffer buffer)
        {
            throw new NotImplementedException();
        }

        public IValueBuffer ToValueBuffer(object entity)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class TestEntityBufferConversionFactory : IEntityBufferConversionFactory
    {
        public IEntityBufferConversion Create(
            Type entityType,
            IReadOnlyList<IConstructor> constructors,
            IReadOnlyList<IColumnMetadata> columns
        )
        {
            return new TestEntityBufferConversion();
        }
    }

    private sealed class TestMappable : IMappable
    {
        public TestMappable(
            string memberName,
            NullableType dataType,
            MappingInfo? mapping,
            ValidationInfo? validationInfo
        )
        {
            MemberName = memberName;
            DataType = dataType;
            Mapping = mapping;
            ValidationInfo = validationInfo;
        }

        public NullableType DataType { get; }
        public MappingInfo? Mapping { get; }
        public string MemberName { get; }
        public ValidationInfo? ValidationInfo { get; }
    }
}