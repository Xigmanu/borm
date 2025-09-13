namespace Borm.Data.Storage;

internal sealed record ResolvedForeignKey(
    Table Table,
    object Value,
    object RawValue,
    bool IsComplexRecord,
    bool ChangeExists
);
