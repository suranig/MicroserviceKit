using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine.Abstractions;

namespace Microservice.Core.TemplateEngine.Services
{
    public class TemplateEngineService
    {
        private readonly ITemplateEngine _templateEngine;
        private readonly ILogger<TemplateEngineService> _logger;

        public TemplateEngineService(
            ITemplateEngine templateEngine,
            ILogger<TemplateEngineService> logger)
        {
            _templateEngine = templateEngine;
            _logger = logger;
        }

        public async Task GenerateAsync(string outputPath, string sourceDirectory, string microserviceName, string @namespace, TemplateConfiguration configuration)
        {
            _logger.LogInformation("Starting template engine service for {MicroserviceName}", microserviceName);

            // Update configuration with passed parameters
            configuration.MicroserviceName = microserviceName;
            configuration.Namespace = @namespace;
            configuration.OutputPath = outputPath;

            if (configuration.ProjectStructure == null)
            {
                configuration.ProjectStructure = new ProjectStructureConfiguration
                {
                    SourceDirectory = sourceDirectory
                };
            }
            else
            {
                configuration.ProjectStructure.SourceDirectory = sourceDirectory;
            }

            var context = new GenerationContext(configuration);
            await _templateEngine.GenerateAsync(context, configuration);

            _logger.LogInformation("Template engine service completed successfully");
        }
    }
} 