using Borm.Data;
using Borm.Properties;
using Borm.Tests.Common;
using Xunit.Abstractions;

namespace Borm.Tests.EndToEnd.Repository;

public sealed class TransactionTest
{
    private readonly ITestOutputHelper _output;

    public TransactionTest(ITestOutputHelper output)
    {
        _output = output;
    }

    public static TheoryData<bool, bool> TruthTableData =>
        new()
        {
            { true, true },
            { true, false },
            { false, true },
            { false, false },
        };

    [Theory]
    [MemberData(nameof(TruthTableData))]
    public void ConcurrencyConflict_NotRecoverable(bool initSaveChanges, bool postTxSaveChanges)
    {
        _output.WriteLine(
            $"{nameof(initSaveChanges)}: {initSaveChanges}\n{nameof(postTxSaveChanges)}: {postTxSaveChanges}"
        );

        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        AddressEntity addressUpdate = new(address.Id, "new_address", null, "bar");

        IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();

        // Act
        addressRepo.Insert(address);
        if (initSaveChanges)
        {
            context.SaveChanges();
        }

        using Transaction transaction0 = context.BeginTransaction();
        using Transaction transaction1 = context.BeginTransaction();

        addressRepo.Update(addressUpdate, transaction0);
        addressRepo.Delete(address, transaction1);

        transaction1.Dispose();
        if (postTxSaveChanges)
        {
            context.SaveChanges();
        }

        Exception? exception = Record.Exception(() => transaction0.Dispose());

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
    }

    [Theory]
    [MemberData(nameof(TruthTableData))]
    public void ConcurrencyConflict_Recoverable(bool initSaveChanges, bool postTxSaveChanges)
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        AddressEntity addressUpdate = new(address.Id, "new_address", null, "bar");

        IEntityRepository<AddressEntity> addressRepo = context.GetRepository<AddressEntity>();

        // Act
        addressRepo.Insert(address);
        if (initSaveChanges)
        {
            context.SaveChanges();
        }

        using Transaction transaction0 = context.BeginTransaction();
        using Transaction transaction1 = context.BeginTransaction();

        addressRepo.Update(addressUpdate, transaction0);
        addressRepo.Update(address, transaction1);

        transaction1.Dispose();
        if (postTxSaveChanges)
        {
            context.SaveChanges();
        }

        Exception? exception = Record.Exception(() => transaction0.Dispose());

        // Assert
        Assert.Null(exception);
    }

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
    public void Exception_InsideOfTransactionScope_WithNullArgument()
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
            repository.Update(null!, transaction);
        });

        // Assert
        Assert.NotNull(exception);
        Assert.IsType<InvalidOperationException>(exception);
        Assert.Equal(Strings.TransactionFailed(), exception.Message);

        IEnumerable<AddressEntity> addresses = repository.Select();
        Assert.Empty(addresses);
    }

    [Fact]
    public void ValidInsertDelete()
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
            repository.Delete(address, transaction);
        }

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Empty(addresses);
    }

    [Fact]
    public void ValidInsertUpdate()
    {
        // Arrange
        DataContext context = DataContextProvider.CreateDataContext();
        context.Initialize();

        AddressEntity address = new(1, "address", "address2", "city");
        AddressEntity addressUpdate = new(address.Id, "new_address", null, "bar");
        IEntityRepository<AddressEntity> repository = context.GetRepository<AddressEntity>();

        // Act
        using (Transaction transaction = context.BeginTransaction())
        {
            repository.Insert(address, transaction);
            repository.Update(addressUpdate, transaction);
        }

        // Assert
        IEnumerable<AddressEntity> addresses = repository.Select();

        Assert.Single(addresses);
        Assert.Equal(addressUpdate, addresses.First());
    }
}
