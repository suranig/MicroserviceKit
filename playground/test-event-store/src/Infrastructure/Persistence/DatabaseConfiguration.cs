using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EventStoreService.Infrastructure.Persistence;

namespace EventStoreService.Infrastructure.Persistence;

public static class DatabaseConfiguration
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (environment.IsDevelopment())
        {
            // Ensure database is created in development
            await context.Database.EnsureCreatedAsync();
        }
        else
        {
            // Apply migrations in production
            await context.Database.MigrateAsync();
        }
    }

    public static string GetConnectionString(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "postgresql" => "Host=localhost;Database={ServiceName}Db;Username=postgres;Password=postgres123",
            "sqlserver" => "Server=localhost;Database={ServiceName}DB;User Id=sa;Password=SqlServer123!;TrustServerCertificate=true",
            "mysql" => "Server=localhost;Database={ServiceName}DB;User=root;Password=mysql123;",
            "sqlite" => "Data Source={ServiceName}.db",
            _ => "Data Source=:memory:"
        };
    }
}