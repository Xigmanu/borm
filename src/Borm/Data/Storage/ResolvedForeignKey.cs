namespace Borm.Data.Storage;

internal sealed record ResolvedForeignKey(
    ITable Parent,
    object Value,
    object RawValue,
    bool IsComplexRecord,
    bool ChangeExists
);