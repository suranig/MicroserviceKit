using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MassTransit;

namespace EventStoreService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Add MassTransit
        services.AddMassTransit(x =>
        {
            // Add consumers from current assembly
            x.AddConsumers(Assembly.GetExecutingAssembly());
            
            // Configure RabbitMQ transport
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });
        
        return services;
    }
}