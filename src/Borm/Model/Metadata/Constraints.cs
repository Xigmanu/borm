namespace Borm.Schema.Metadata;

[Flags]
internal enum Constraints
{
    None = 0,
    PrimaryKey = 2,
    Unique = 4,
    AllowDbNull = 8,
}
