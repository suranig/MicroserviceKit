using Microsoft.Extensions.DependencyInjection;
using Microservice.Core.TemplateEngine;
using Microservice.Core.TemplateEngine.Abstractions;
using System;

namespace Microservice.Core.TemplateEngine
{
    public static class TemplateEngineExtensions
    {
        public static IServiceCollection AddTemplateEngine(this IServiceCollection services)
        {
            services.AddSingleton<ITemplateEngine, TemplateEngine>();
            
            // Register modules directly by type - they will be loaded at runtime when available
            // We can't use Type.GetType() with string names because assemblies may not be loaded yet

            return services;
        }
    }
} 