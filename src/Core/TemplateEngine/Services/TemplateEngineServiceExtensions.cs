using Microsoft.Extensions.DependencyInjection;
using Microservice.Core.TemplateEngine.Services;

namespace Microservice.Core.TemplateEngine.Services
{
    public static class TemplateEngineServiceExtensions
    {
        public static IServiceCollection AddTemplateEngineService(this IServiceCollection services)
        {
            services.AddSingleton<TemplateEngineService>();
            return services;
        }
    }
} 