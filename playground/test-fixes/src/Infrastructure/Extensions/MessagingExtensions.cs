using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestService.Application.Common.Events;
using TestService.Infrastructure.Messaging;
using TestService.Infrastructure.Messaging.Configuration;
using TestService.Infrastructure.Messaging.Publishers;

namespace TestService.Infrastructure.Extensions;

public static class MessagingExtensions
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        // Add RabbitMQ
        services.AddRabbitMQ(configuration);

        // Add event dispatcher
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        // Add domain event publisher
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        // Add outbox pattern
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddHostedService<OutboxProcessor>();

        // Add event handlers
        RegisterEventHandlers(services);

        return services;
    }

    private static void RegisterEventHandlers(IServiceCollection services)
    {
        // Auto-register all event handlers from the application assembly
        var applicationAssembly = typeof(TestService.Application.AssemblyReference).Assembly;
        
        var eventHandlerTypes = applicationAssembly.GetTypes()
            .Where(t => t.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .Where(t => !t.IsAbstract && !t.IsInterface);

        foreach (var handlerType in eventHandlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>));

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, handlerType);
            }
        }
    }
}