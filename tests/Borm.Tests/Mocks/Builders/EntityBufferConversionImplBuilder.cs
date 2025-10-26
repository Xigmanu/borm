using Borm.Data.Storage;
using Borm.Model.Metadata.Conversion;
using Moq;

namespace Borm.Tests.Mocks.Builders;

internal sealed class EntityBufferConversionImplBuilder
{
    private readonly Mock<IEntityBufferConversion> _mock = new();

    public IEntityBufferConversion Build() => _mock.Object;

    public EntityBufferConversionImplBuilder MaterializeEntity(
        Func<IValueBuffer, object> materializeEntity
    )
    {
        _mock
            .Setup(c => c.MaterializeEntity(It.IsAny<IValueBuffer>()))
            .Returns<IValueBuffer>(buf => materializeEntity(buf));
        return this;
    }

    public EntityBufferConversionImplBuilder ToValueBuffer(Func<object, IValueBuffer> toValueBuffer)
    {
        _mock
            .Setup(c => c.ToValueBuffer(It.IsAny<object>()))
            .Returns<object>(entity => toValueBuffer(entity));
        return this;
    }
}
