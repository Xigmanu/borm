namespace Borm.Model;

/// <summary>
/// Specifies that a property defines a primary key column.
/// </summary>
/// <remarks>
/// Every entity must have one primary key.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class PrimaryKeyAttribute : ColumnAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimaryKeyAttribute"/> class
    /// with the specified column index, and column name.
    /// </summary>
    /// <param name="index">
    /// Zero-based index of a column in a table.
    /// Must be greater than or equal to zero.
    /// </param>
    /// <param name="name">
    /// The name of the column in the table.
    /// </param>
    public PrimaryKeyAttribute(int index, string name)
        : base(index, name) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimaryKeyAttribute"/> class
    /// with the specified column index.
    /// </summary>
    /// <param name="index">
    /// Zero-based index of a column in a table.
    /// Must be greater than or equal to zero.
    /// </param>
    public PrimaryKeyAttribute(int index)
        : base(index) { }
}
