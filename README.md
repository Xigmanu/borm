# Borm

Borm is a lightweight, code-first ORM framework for .NET. Currently, it only supports the SQLite database via a provider plugin API. It also supports pure in-memory configuration.

## Usage

The following chapters describe a simple workflow for using Borm, from entity creation to transaction management.

### Entities

Tables are described using entities, which are defined and configured using various attributes.

```csharp
[Entity("addresses")]
public sealed class Address(Guid id, string address, string? address_1, string city, string postal_code)
{
    [PrimaryKey(0, "id")]
    public Guid Id { get; } = id;

    [Column(1, "address")]
    public string Address { get; } = address;

    [Column(2, "address_1")]
    public string? Address1 { get; } = address_1;

    [Column(3, "city")]
    public string City { get; } = city;

    [Column(4, "postal_code")]
    public string Code { get; } = postal_code;
} 
```

#### Relations

Entity relations can be simple or complex.

For a simple relation, a property of the parent's primary key type must be defined in the child entity.

```csharp
[ForeignKey(1, "address_id", typeof(AddressEntity))]
public int Address { get }
```

Complex relations differ in that the type of the foreign key property is the same type as the parent entity.\
**Note**: when querying an entity with such a relationship, the parent object will be constructed first due to the composite relationship between classes. Misuse of this can lead to performance issues.

```csharp
[ForeignKey(1, "address_id", typeof(AddressEntity))]
public AddressEntity Address { get }
```

Internally, the child table still refers to the foreign key of the parent table. That's why the column name is still *address_id*.

#### Optional: Validating entities

In addition to constraint validation, validators can be used to ensure the correctness of entity objects. A validator is executed before any data is written.

```csharp
public sealed class AddressEntityValidator : IEntityValidator<AddressEntity>
{
    void Validate(AddressEntity entity)
    {
        if (entity.Address == string.Empty)
            throw new InvalidOperationException("Address cannot be an empty string");
    }
}
```

#### Creating a model

The `EntityModel` class allows you to register entities alongside their validators.

```csharp
EntityModel model = new();
model.AddEntity(typeof(AddressEntity), new AddressEntityValidator());
```

### DataContext

The model is then used to create a `BormConfig` instance, which in turn is used to create a `DataContext` instance.

```csharp
BormConfig config = new BormConfig.Builder()
                    .Model(model)
                    .InMemory()
                    .Build();

DataContext context = new(config);
context.Initialize();
```

All table operations are performed using specialised repositories. These act as an interface for entity tables. The `DataContext` class can be used to create these repositories.

```csharp
IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();

AddressEntity address = new(Guid.NewGuid(), "foo", null, "bar", "12345");
addressRepo.Insert(address);
```

Repository write operations do not write changes to the data source. To do this, the `DataContext` has a `SaveChanges` method.

```csharp
context.SaveChanges();
```

### Transactions

Every direct write operation is performed within a transaction internally. However, it is possible to define an explicit, disposable transaction.

```csharp
using (Transaction transaction = context.BeginTransaction()) 
{
    addressRepo.Insert(address, transaction);
    addressRepo.Delete(oldAddress, transaction);
}

context.SaveChanges();
```

Once the `Dispose` method has been called, all changes will be committed to the primary context. If any operations fail, no changes will be written and the exception will propagate further.

If another transaction modifies the same record before this transaction is committed, the operations within its scope will be re-executed in the same order. This will happen up to three times, after which the transaction will fail.
