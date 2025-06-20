using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TestService.Domain.Test;
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
        if (!_context.Tests.Any())
        {
            var testItems = TestDataBuilder.CreateMultipleTests(10);
            _context.Tests.AddRange(testItems);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Test> CreateTestAsync()
    {
        var test = TestDataBuilder.CreateTest();
        _context.Tests.Add(test);
        await _context.SaveChangesAsync();
        return test;
    }

    public async Task SeedMultipleTestsAsync(int count)
    {
        var tests = TestDataBuilder.CreateMultipleTests(count);
        _context.Tests.AddRange(tests);
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