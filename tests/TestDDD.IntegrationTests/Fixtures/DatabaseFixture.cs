using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ECommerce.OrderService.Domain.Order;
using ECommerce.OrderService.Infrastructure.Persistence;
using ECommerce.OrderService.Integration.Tests.Helpers;

namespace ECommerce.OrderService.Integration.Tests.Fixtures;

public class DatabaseFixture : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public DatabaseFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=:memory:";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid())
            .EnableSensitiveDataLogging()
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public async Task SeedTestDataAsync()
    {
        if (!_context.Orders.Any())
        {
            var testItems = TestDataBuilder.CreateMultipleOrders(10);
            _context.Orders.AddRange(testItems);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Order> CreateOrderAsync()
    {
        var order = TestDataBuilder.CreateOrder();
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task SeedMultipleOrdersAsync(int count)
    {
        var orders = TestDataBuilder.CreateMultipleOrders(count);
        _context.Orders.AddRange(orders);
        await _context.SaveChangesAsync();
    }

    public async Task<Customer> CreateCustomerAsync()
    {
        var customer = TestDataBuilder.CreateCustomer();
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return customer;
    }

    public async Task SeedMultipleCustomersAsync(int count)
    {
        var customers = TestDataBuilder.CreateMultipleCustomers(count);
        _context.Customers.AddRange(customers);
        await _context.SaveChangesAsync();
    }

    public async Task CleanupAsync()
    {
        _context.Database.EnsureDeleted();
        await _context.Database.EnsureCreatedAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}