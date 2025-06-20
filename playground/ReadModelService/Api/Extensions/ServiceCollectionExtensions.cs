using Microsoft.Extensions.DependencyInjection;

namespace ReadModelService.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {
            // Register API services
            return services;
        }
    }
}