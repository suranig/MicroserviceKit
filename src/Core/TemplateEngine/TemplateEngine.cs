using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine.Abstractions;

namespace Microservice.Core.TemplateEngine
{
    public class TemplateEngine : ITemplateEngine
    {
        private readonly ILogger<TemplateEngine> _logger;
        private readonly IEnumerable<ITemplateModule> _modules;

        public TemplateEngine(
            ILogger<TemplateEngine> logger,
            IEnumerable<ITemplateModule> modules)
        {
            _logger = logger;
            _modules = modules;
        }

        public async Task GenerateAsync(GenerationContext context, TemplateConfiguration configuration)
        {
            _logger.LogInformation("Starting template generation for {MicroserviceName}", configuration.MicroserviceName);

            // Create MicroserviceGenerator to handle solution file and orchestration
            var microserviceGenerator = new MicroserviceGenerator(context, configuration, _logger, _modules);
            await microserviceGenerator.GenerateAsync();

            _logger.LogInformation("Template generation completed successfully");
        }
    }
} 