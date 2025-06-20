using SimpleService.Infrastructure.Messaging.Configuration;
using SimpleService.Infrastructure.Messaging.Publishers;
using SimpleService.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleService.Infrastructure.Persistence;
using SimpleService.Infrastructure.Repositories;

namespace SimpleService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        // Add repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Add health checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        
        // Add messaging
        services.AddRabbitMQ(configuration);
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddHostedService<OutboxProcessor>;

        return services;
    }
}