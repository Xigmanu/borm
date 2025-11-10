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
        List<AddressEntity> addresses = addressRepo.Select().ToList();

        Assert.Single(addresses);
        Assert.Equal(address, addresses[0]);

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
        Assert.IsType<RecordNotFoundException>(inner);
        Assert.Equal(Strings.RowNotFound("persons", invalidPerson.Id), inner.Message);

        List<PersonEntity> persons = personRepo.Select().ToList();

        Assert.Single(persons);
        Assert.Equal(person, persons[0]);
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
            IsActive = true
        };

        IEntityRepository<PersonEntity> personsRepo = context.GetRepository<PersonEntity>();
        IEntityRepository<EmployeeEntity> employeeRepo = context.GetRepository<EmployeeEntity>();

        // Act
        personsRepo.Insert(person);
        employeeRepo.Insert(employee);

        employeeRepo.Delete(employee);

        // Assert
        IEnumerable<EmployeeEntity> employees = employeeRepo.Select();
        List<PersonEntity> persons = personsRepo.Select().ToList();

        Assert.Empty(employees);
        Assert.Single(persons);
        Assert.Equal(person, persons[0]);
    }

    [Fact]
    public void WithSetNullReferentialAction()
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
        addressRepo.Delete(address);

        // Assert
        IEnumerable<AddressEntity> addresses = addressRepo.Select();
        List<PersonEntity> persons = personRepo.Select().ToList();

        Assert.Empty(addresses);
        Assert.Single(persons);

        PersonEntity actual = persons[0];
        Assert.Null(actual.Address);
    }

    [Fact]
    public void WithCascadeReferentialAction()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        PersonEntity person = new(1, "name", 42.619, address);
        EmployeeEntity employee = new()
        {
            Id = 1,
            Person = person.Id,
            IsActive = true
        };
        IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();
        IEntityRepository<PersonEntity> personRepo = context.GetRepository<PersonEntity>();
        IEntityRepository<EmployeeEntity> employeeRepo = context.GetRepository<EmployeeEntity>();

        // Act
        personRepo.Insert(person);
        employeeRepo.Insert(employee);

        personRepo.Delete(person);

        // Assert
        List<AddressEntity> addresses = addressRepo.Select().ToList();
        IEnumerable<PersonEntity> persons = personRepo.Select();
        IEnumerable<EmployeeEntity> employees = employeeRepo.Select();

        Assert.Empty(persons);
        Assert.Single(addresses);
        Assert.Empty(employees);

        Assert.Equal(address, addresses[0]);
    }
}