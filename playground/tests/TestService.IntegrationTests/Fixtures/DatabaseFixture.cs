using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TestService.Domain.Product;
using TestService.Infrastructure.Persistence;
using TestService.Integration.Tests.Helpers;

namespace TestService.Integration.Tests.Fixtures;

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
        if (!_context.Products.Any())
        {
            var testItems = TestDataBuilder.CreateMultipleProducts(10);
            _context.Products.AddRange(testItems);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Product> CreateProductAsync()
    {
        var product = TestDataBuilder.CreateProduct();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task SeedMultipleProductsAsync(int count)
    {
        var products = TestDataBuilder.CreateMultipleProducts(count);
        _context.Products.AddRange(products);
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