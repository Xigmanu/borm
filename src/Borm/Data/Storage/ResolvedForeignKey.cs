namespace Borm.Data.Storage;

internal sealed record ResolvedForeignKey(
    ITable Parent,
    object PrimaryKey,
    object RawValue,
    bool IsComplexRecord,
    bool ChangeExists
);