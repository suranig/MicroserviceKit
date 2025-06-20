using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventStoreService.Infrastructure.Persistence;
using EventStoreService.Infrastructure.Repositories;

namespace EventStoreService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase("InMemoryDatabase");
            options.EnableSensitiveDataLogging(true);
        });

        // Add repositories
        services.AddScoped<IEventRepository, EventRepository>();

        // Add health checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }
}