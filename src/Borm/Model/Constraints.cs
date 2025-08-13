namespace Borm.Model;

[Flags]
public enum Constraints
{
    None = 0,
    PrimaryKey = 2,
    Unique = 4,
    AllowDbNull = 8,
}
