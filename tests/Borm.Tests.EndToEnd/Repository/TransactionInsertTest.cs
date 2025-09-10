using Borm.Data;
using Borm.Properties;
using Borm.Tests.Common;

namespace Borm.Tests.EndToEnd.Repository;

public sealed class TransactionInsertTest
{
    [Fact]
    public void Exception_InsideOfTransactionScope()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address0 = new(1, "address", "address2", "city");
        AddressEntity address1 = new(2, string.Empty, "address2", "city");

        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        Exception? exception = Record.Exception(() =>
        {
            using Transaction transaction = context.BeginTransaction();
            repository.Insert(address0, transaction);
            repository.Insert(address1, transaction);
        });

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        IEnumerable<AddressEntity> addresses = repository.Select();
        Assert.Empty(addresses);
    }

    [Fact]
    public void Exception_InsideOfTransactionScope_WithSecondNullArgument()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address0 = new(1, "address", "address2", "city");

        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        Exception? exception = Record.Exception(() =>
        {
            using Transaction transaction = context.BeginTransaction();
            repository.Insert(address0, transaction);
            repository.Insert(null!, transaction);
        });

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        IEnumerable<AddressEntity> addresses = repository.Select();
        Assert.Empty(addresses);
    }

    [Fact]
    public void ValidSimpleEntity()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        using (Transaction transaction = context.BeginTransaction())
        {
            repository.Insert(address, transaction);
        }

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Single(addresses);
        Assert.Equal(address, addresses.First());
    }
}
