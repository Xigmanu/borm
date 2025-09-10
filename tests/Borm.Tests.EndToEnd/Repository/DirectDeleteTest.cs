using Borm.Data;
using Borm.Data.Storage;
using Borm.Properties;
using Borm.Tests.Common;

namespace Borm.Tests.EndToEnd.Repository;

public sealed class DirectDeleteTest
{
    [Fact]
    public void NullEntity()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        Exception? exception = Record.Exception(() => repository.Delete(null!));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<ArgumentNullException>(inner);
    }

    [Fact]
    public void ValidComplexRelationalEntity_WithForeignKeyCollision()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        PersonEntity person = new(1, "name", 42.619, address);
        IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();

        // Act
        personRepo.Insert(person);
        personRepo.Delete(person);

        // Assert
        IEnumerable<AddressEntity> addresses = addressRepo.Select();

        Assert.Single(addresses);
        Assert.Equal(address, addresses.First());

        IEnumerable<PersonEntity> persons = personRepo.Select();
        Assert.Empty(persons);
    }

    [Fact]
    public void ValidSimpleEntity_WithNoPrimaryKeyCollision()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        PersonEntity person = new(1, "name", 42.619, null);
        PersonEntity invalidPerson = new(2, "name", 42.619, null);
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();

        // Act
        personRepo.Insert(person);
        Exception? exception = Record.Exception(() => personRepo.Delete(invalidPerson));

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);

        Exception? inner = exception.InnerException;
        Assert.NotNull(inner);
        Assert.IsType<RowNotFoundException>(inner);
        Assert.Equal(Strings.RowNotFound("persons", invalidPerson.Id), inner.Message);

        IEnumerable<PersonEntity> persons = personRepo.Select();

        Assert.Single(persons);
        Assert.Equal(person, persons.First());
    }

    [Fact]
    public void ValidSimpleEntity_WithoutSavingChanges()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(address);
        repository.Delete(address);

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Empty(addresses);
    }

    [Fact]
    public void ValidSimpleEntity_WithSavingChanges()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        repository.Insert(address);
        context.SaveChanges();
        repository.Delete(address);

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Single(addresses);
        Assert.Equal(address, addresses.First());
    }

    [Fact]
    public void ValidSimpleRelationalEntity()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        PersonEntity person = new(1, "name", 42.619, null);
        EmployeeEntity employee = new()
        {
            Id = 1,
            Person = person.Id,
            IsActive = true,
        };

        IEntityRepository<PersonEntity> personsRepo = context.GetRepository<PersonEntity>();
        IEntityRepository<EmployeeEntity> employeeRepo = context.GetRepository<EmployeeEntity>();

        // Act
        personsRepo.Insert(person);
        employeeRepo.Insert(employee);

        employeeRepo.Delete(employee);

        // Assert
        IEnumerable<EmployeeEntity> employees = employeeRepo.Select();
        IEnumerable<PersonEntity> persons = personsRepo.Select();

        Assert.Empty(employees);
        Assert.Single(persons);
        Assert.Equal(person, persons.First());
    }
}
