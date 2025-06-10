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
            
            // Register modules using type strings to avoid circular references
            RegisterModule(services, "Microservice.Modules.DDD.DDDModule");
            RegisterModule(services, "Microservice.Modules.Application.ApplicationModule");
            RegisterModule(services, "Microservice.Modules.Infrastructure.InfrastructureModule");
            RegisterModule(services, "Microservice.Modules.Api.ApiModule");
            RegisterModule(services, "Microservice.Modules.Tests.TestsModule");
            RegisterModule(services, "Microservice.Modules.Docker.DockerModule");
            RegisterModule(services, "Microservice.Modules.Messaging.MessagingModule");

            return services;
        }

        private static void RegisterModule(IServiceCollection services, string typeName)
        {
            var moduleType = Type.GetType(typeName);
            if (moduleType != null)
            {
                services.AddSingleton(typeof(ITemplateModule), moduleType);
            }
        }
    }
} 