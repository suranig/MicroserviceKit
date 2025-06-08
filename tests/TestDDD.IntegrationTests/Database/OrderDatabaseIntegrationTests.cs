using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ECommerce.OrderService.Domain.Order;
using ECommerce.OrderService.Infrastructure.Persistence;
using ECommerce.OrderService.Infrastructure.Repositories;
using ECommerce.OrderService.Integration.Tests.Fixtures;
using ECommerce.OrderService.Integration.Tests.Helpers;

namespace ECommerce.OrderService.Integration.Tests.Database;

public class OrderDatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public OrderDatabaseIntegrationTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Repository_Add_ShouldPersistOrderToDatabase()
    {
        // Arrange
        using var context = _databaseFixture.CreateDbContext();
        var repository = new OrderRepository(context);
        var order = TestDataBuilder.CreateOrder();

        // Act
        await repository.AddAsync(order);
        await context.SaveChangesAsync();

        // Assert
        var savedOrder = await context.Orders.FirstOrDefaultAsync(x => x.Id == order.Id);
        savedOrder.Should().NotBeNull();
        savedOrder!.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task Repository_GetById_ShouldRetrieveOrderFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateOrderAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new OrderRepository(context);

        // Act
        var result = await repository.GetByIdAsync(testData.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task Repository_Update_ShouldModifyOrderInDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateOrderAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new OrderRepository(context);

        // Act
        var order = await repository.GetByIdAsync(testData.Id);
        order.Should().NotBeNull();
        
        // Modify entity (add specific property modifications based on aggregate)
        repository.Update(order!);
        await context.SaveChangesAsync();

        // Assert
        var updatedOrder = await repository.GetByIdAsync(testData.Id);
        updatedOrder.Should().NotBeNull();
        // Add specific assertions for modified properties
    }

    [Fact]
    public async Task Repository_Delete_ShouldRemoveOrderFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateOrderAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new OrderRepository(context);

        // Act
        var order = await repository.GetByIdAsync(testData.Id);
        order.Should().NotBeNull();
        
        repository.Delete(order!);
        await context.SaveChangesAsync();

        // Assert
        var deletedOrder = await repository.GetByIdAsync(testData.Id);
        deletedOrder.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetAll_ShouldReturnAllOrders()
    {
        // Arrange
        await _databaseFixture.SeedMultipleOrdersAsync(5);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new OrderRepository(context);

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
        await _databaseFixture.SeedMultipleOrdersAsync(20);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new OrderRepository(context);

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
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Orders'");
        tableExists.First().Should().Be(1);
    }

    [Fact]
    public async Task DbContext_ConcurrencyHandling_ShouldHandleOptimisticLocking()
    {
        // Arrange
        var testData = await _databaseFixture.CreateOrderAsync();
        
        using var context1 = _databaseFixture.CreateDbContext();
        using var context2 = _databaseFixture.CreateDbContext();
        
        var repository1 = new OrderRepository(context1);
        var repository2 = new OrderRepository(context2);

        // Act
        var order1 = await repository1.GetByIdAsync(testData.Id);
        var order2 = await repository2.GetByIdAsync(testData.Id);

        order1.Should().NotBeNull();
        order2.Should().NotBeNull();

        // Modify both entities
        repository1.Update(order1!);
        repository2.Update(order2!);

        // Save first context
        await context1.SaveChangesAsync();

        // Assert
        // Second context should throw concurrency exception
        var act = async () => await context2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}