using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ECommerce.OrderService.Domain.Customer;
using ECommerce.OrderService.Infrastructure.Persistence;
using ECommerce.OrderService.Infrastructure.Repositories;
using ECommerce.OrderService.Integration.Tests.Fixtures;
using ECommerce.OrderService.Integration.Tests.Helpers;

namespace ECommerce.OrderService.Integration.Tests.Database;

public class CustomerDatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public CustomerDatabaseIntegrationTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Repository_Add_ShouldPersistCustomerToDatabase()
    {
        // Arrange
        using var context = _databaseFixture.CreateDbContext();
        var repository = new CustomerRepository(context);
        var customer = TestDataBuilder.CreateCustomer();

        // Act
        await repository.AddAsync(customer);
        await context.SaveChangesAsync();

        // Assert
        var savedCustomer = await context.Customers.FirstOrDefaultAsync(x => x.Id == customer.Id);
        savedCustomer.Should().NotBeNull();
        savedCustomer!.Id.Should().Be(customer.Id);
    }

    [Fact]
    public async Task Repository_GetById_ShouldRetrieveCustomerFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateCustomerAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new CustomerRepository(context);

        // Act
        var result = await repository.GetByIdAsync(testData.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task Repository_Update_ShouldModifyCustomerInDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateCustomerAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new CustomerRepository(context);

        // Act
        var customer = await repository.GetByIdAsync(testData.Id);
        customer.Should().NotBeNull();
        
        // Modify entity (add specific property modifications based on aggregate)
        repository.Update(customer!);
        await context.SaveChangesAsync();

        // Assert
        var updatedCustomer = await repository.GetByIdAsync(testData.Id);
        updatedCustomer.Should().NotBeNull();
        // Add specific assertions for modified properties
    }

    [Fact]
    public async Task Repository_Delete_ShouldRemoveCustomerFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateCustomerAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new CustomerRepository(context);

        // Act
        var customer = await repository.GetByIdAsync(testData.Id);
        customer.Should().NotBeNull();
        
        repository.Delete(customer!);
        await context.SaveChangesAsync();

        // Assert
        var deletedCustomer = await repository.GetByIdAsync(testData.Id);
        deletedCustomer.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetAll_ShouldReturnAllCustomers()
    {
        // Arrange
        await _databaseFixture.SeedMultipleCustomersAsync(5);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new CustomerRepository(context);

        // Act
        var results = await repository.GetAllAsync();

        // Assert
        results.Should().NotBeEmpty();
        results.Count().Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public async Task Repository_GetWithPaging_ShouldReturnPagedResults()
    {
        // Arrange
        await _databaseFixture.SeedMultipleCustomersAsync(20);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new CustomerRepository(context);

        // Act
        var results = await repository.GetPagedAsync(1, 10);

        // Assert
        results.Should().NotBeNull();
        results.Items.Should().HaveCount(10);
        results.TotalCount.Should().BeGreaterOrEqualTo(20);
        results.Page.Should().Be(1);
        results.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task DbContext_Migrations_ShouldCreateCorrectSchema()
    {
        // Arrange & Act
        using var context = _databaseFixture.CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        // Assert
        var tableExists = await context.Database.GetDbConnection().QueryAsync<int>(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Customers'");
        tableExists.First().Should().Be(1);
    }

    [Fact]
    public async Task DbContext_ConcurrencyHandling_ShouldHandleOptimisticLocking()
    {
        // Arrange
        var testData = await _databaseFixture.CreateCustomerAsync();
        
        using var context1 = _databaseFixture.CreateDbContext();
        using var context2 = _databaseFixture.CreateDbContext();
        
        var repository1 = new CustomerRepository(context1);
        var repository2 = new CustomerRepository(context2);

        // Act
        var customer1 = await repository1.GetByIdAsync(testData.Id);
        var customer2 = await repository2.GetByIdAsync(testData.Id);

        customer1.Should().NotBeNull();
        customer2.Should().NotBeNull();

        // Modify both entities
        repository1.Update(customer1!);
        repository2.Update(customer2!);

        // Save first context
        await context1.SaveChangesAsync();

        // Assert
        // Second context should throw concurrency exception
        var act = async () => await context2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}