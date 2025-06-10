using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Company.TestService.Domain.Item;
using Company.TestService.Infrastructure.Persistence;
using Company.TestService.Integration.Tests.Helpers;

namespace Company.TestService.Integration.Tests.Fixtures;

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
        if (!_context.Items.Any())
        {
            var testItems = TestDataBuilder.CreateMultipleItems(10);
            _context.Items.AddRange(testItems);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Item> CreateItemAsync()
    {
        var item = TestDataBuilder.CreateItem();
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task SeedMultipleItemsAsync(int count)
    {
        var items = TestDataBuilder.CreateMultipleItems(count);
        _context.Items.AddRange(items);
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