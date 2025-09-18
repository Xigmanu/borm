namespace Borm.Model;

/// <summary>
/// Specifies that a property describes a foreign key column.
/// </summary>
/// <remarks>
/// The referenced type must be marked with the <see cref="EntityAttribute"/>, otherwise an exception will be thrown.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ForeignKeyAttribute : ColumnAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class
    /// with the specified column index, column name, and referenced entity type.
    /// </summary>
    /// <param name="index">
    /// Zero-based index of a column in a table.
    /// Must be greater than or equal to zero.
    /// </param>
    /// <param name="name">
    /// The name of the column in the table.
    /// </param>
    /// <param name="reference">
    /// The <see cref="Type"/> of the entity that this column references.
    /// It must be a type marked with the <see cref="EntityAttribute"/>.
    /// </param>
    public ForeignKeyAttribute(int index, string name, Type reference)
        : base(index, name)
    {
        Reference = reference;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class
    /// with the specified column index, and referenced entity type.
    /// </summary>
    /// <param name="index">
    /// Zero-based index of a column in a table.
    /// Must be greater than or equal to zero.
    /// </param>
    /// <param name="reference">
    /// The <see cref="Type"/> of the entity that this column references.
    /// It must be a type marked with the <see cref="EntityAttribute"/>.
    /// </param>
    public ForeignKeyAttribute(int index, Type reference)
        : base(index)
    {
        Reference = reference;
    }

    public ReferentialAction OnDelete { get; set; } = ReferentialAction.NoAction;

    /// <summary>
    /// The entity type that this column references.
    /// </summary>
    public Type Reference { get; }
}
