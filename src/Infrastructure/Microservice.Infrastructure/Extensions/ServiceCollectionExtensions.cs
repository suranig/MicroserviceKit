using Microsoft.Extensions.DependencyInjection;
using Microservice.Domain.Interfaces;
using Microservice.Infrastructure.Repositories;

namespace Microservice.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();
        return services;
    }
}
