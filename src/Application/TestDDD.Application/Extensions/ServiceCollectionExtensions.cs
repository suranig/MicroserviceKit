using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Wolverine;

namespace ECommerce.OrderService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Add Wolverine behaviors
        services.AddScoped(typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(LoggingBehavior<,>));
        
        return services;
    }
}