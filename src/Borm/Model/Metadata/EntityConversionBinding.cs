using Borm.Data;

namespace Borm.Model.Metadata;

internal sealed class EntityConversionBinding
{
    public static readonly EntityConversionBinding Empty = new(
        (v) => throw new NotImplementedException(),
        (o) => throw new NotImplementedException()
    );

    public EntityConversionBinding(
        Func<ValueBuffer, object> materializeEntity,
        Func<object, ValueBuffer> convertToValueBuffer
    )
    {
        MaterializeEntity = materializeEntity;
        ToValueBuffer = convertToValueBuffer;
    }

    internal Func<object, ValueBuffer> ToValueBuffer { get; }

    internal Func<ValueBuffer, object> MaterializeEntity { get; }
}
