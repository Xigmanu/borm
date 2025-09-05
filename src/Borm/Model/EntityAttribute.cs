namespace Borm.Model;

/// <summary>
/// Specifies that a class describes a database entity.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EntityAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityAttribute"/> class,
    /// using the class name as the default table name.
    /// </summary>
    public EntityAttribute() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityAttribute"/> class
    /// with a provided table name.
    /// </summary>
    /// <remarks>
    /// The entity name must be unique for each <see cref="DataContext"/>.
    /// If two entities share the same name, an exception is thrown.
    /// </remarks>
    ///
    /// <param name="name">The name of the table to which the class is mapped.</param>
    public EntityAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the name of the database table mapped to the entity.
    /// If not specified, the class name is used by convention.
    /// </summary>
    public string? Name { get; }
}
