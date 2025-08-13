using Borm.Data;

namespace Borm.Model.Metadata;

internal sealed class ConversionBinding
{
    public static readonly ConversionBinding Empty = new(
        (v) => throw new NotImplementedException(),
        (o) => throw new NotImplementedException()
    );

    public ConversionBinding(
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
