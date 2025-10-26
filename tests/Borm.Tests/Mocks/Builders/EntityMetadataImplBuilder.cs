using Borm.Model.Metadata;
using Borm.Model.Metadata.Conversion;
using Moq;

namespace Borm.Tests.Mocks.Builders;

internal sealed class EntityMetadataImplBuilder
{
    private readonly Mock<IEntityMetadata> _mock = new();

    public IEntityMetadata Build() => _mock.Object;

    public EntityMetadataImplBuilder Columns(List<IColumnMetadata> columns)
    {
        _mock.SetupGet(e => e.Columns).Returns(columns.AsReadOnly());
        return this;
    }

    public EntityMetadataImplBuilder Conversion(IEntityBufferConversion conversion)
    {
        _mock.SetupGet(e => e.Conversion).Returns(conversion);
        return this;
    }

    public EntityMetadataImplBuilder Name(string name)
    {
        _mock.SetupGet(e => e.Name).Returns(name);
        return this;
    }

    public EntityMetadataImplBuilder PrimaryKey(IColumnMetadata primaryKey)
    {
        _mock.SetupGet(e => e.PrimaryKey).Returns(primaryKey);
        return this;
    }

    public EntityMetadataImplBuilder Type(Type entityType)
    {
        _mock.SetupGet(e => e.Type).Returns(entityType);
        return this;
    }

    public EntityMetadataImplBuilder Validate(Action<object> validate)
    {
        _mock
            .Setup(e => e.Validate(It.IsAny<object>()))
            .Callback<object>(entity => validate(entity));
        return this;
    }
}
