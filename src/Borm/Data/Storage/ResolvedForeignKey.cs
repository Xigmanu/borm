using Borm.Model;

namespace Borm.Data.Storage;

internal sealed record ResolvedForeignKey(
    Table Parent,
    object PrimaryKey,
    object RawValue,
    bool IsComplexRecord,
    bool ChangeExists,
    ReferentialAction OnDelete,
    ReferentialAction OnUpdate
);
