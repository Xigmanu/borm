using Borm.Data.Storage;

namespace Borm.Model.Conversion;

internal sealed class EntityBufferConversion : IEntityBufferConversion
{
    /// <summary>
    /// This global is for testing purposes only.
    /// </summary>
    public static readonly EntityBufferConversion Empty = new(
        (v) => throw new NotImplementedException(),
        (o) => throw new NotImplementedException()
    );

    private readonly Func<IValueBuffer, object> _materializeEntity;
    private readonly Func<object, IValueBuffer> _toValueBuffer;

    public EntityBufferConversion(
        Func<IValueBuffer, object> materializeEntity,
        Func<object, IValueBuffer> convertToValueBuffer
    )
    {
        _materializeEntity = materializeEntity;
        _toValueBuffer = convertToValueBuffer;
    }

    public object MaterializeEntity(IValueBuffer buffer)
    {
        return _materializeEntity(buffer);
    }

    public IValueBuffer ToValueBuffer(object entity)
    {
        return _toValueBuffer(entity);
    }
}
