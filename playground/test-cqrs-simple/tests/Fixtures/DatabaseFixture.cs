using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SimpleService.Domain.User;
using SimpleService.Infrastructure.Persistence;
using SimpleService.Integration.Tests.Helpers;

namespace SimpleService.Integration.Tests.Fixtures;

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
        if (!_context.Users.Any())
        {
            var testItems = TestDataBuilder.CreateMultipleUsers(10);
            _context.Users.AddRange(testItems);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<User> CreateUserAsync()
    {
        var user = TestDataBuilder.CreateUser();
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task SeedMultipleUsersAsync(int count)
    {
        var users = TestDataBuilder.CreateMultipleUsers(count);
        _context.Users.AddRange(users);
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