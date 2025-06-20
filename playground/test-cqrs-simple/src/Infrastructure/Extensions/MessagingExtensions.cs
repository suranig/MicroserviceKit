using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleService.Application.Common.Events;
using SimpleService.Infrastructure.Messaging;
using SimpleService.Infrastructure.Messaging.Configuration;
using SimpleService.Infrastructure.Messaging.Publishers;

namespace SimpleService.Infrastructure.Extensions;

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
        var applicationAssembly = typeof(SimpleService.Application.AssemblyReference).Assembly;
        
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