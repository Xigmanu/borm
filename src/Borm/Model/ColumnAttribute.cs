using Borm.Properties;

namespace Borm.Model;

/// <summary>
/// Specifies that a property describes a table column.
/// </summary>
/// <remarks>
///     <para>
///         Applying this attribute to a property, that is not declared in a class marked with
///         <see cref="EntityAttribute"/> will have no effect.
///     </para>
///     <para>
///         Each column in a table must have a unique name. 
///         If no name is specified, the property name is used instead.
///     </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnAttribute"/> class
    /// with the specified column index.
    /// </summary>
    /// <param name="index">
    /// Zero-based index of a column in a table.
    /// Must be greater than or equal to zero.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="index"/> is less than zero.
    /// </exception>
    public ColumnAttribute(int index)
    {
        Index = index < 0 ? throw new ArgumentException(Strings.InvalidColumnIndex()) : index;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColumnAttribute"/> class
    /// with the specified column index and name.
    /// </summary>
    /// <param name="index">
    /// Zero-based index of a column in a table.
    /// Must be greater than or equal to zero.
    /// </param>
    /// <param name="name">
    /// The name of the column in the table.
    /// </param>
    public ColumnAttribute(int index, string name)
        : this(index)
    {
        Name = name;
    }

    /// <summary>
    /// Index of the column.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Name of the column.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Flag, that indicates whether the values in this column are unique across the entire table.
    /// Default is <see langword="false"/>.
    /// </summary>
    public bool IsUnique { get; set; } = false;
}
