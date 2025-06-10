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

            // Generate modules
            foreach (var module in _modules)
            {
                if (module.IsEnabled(configuration))
                {
                    _logger.LogInformation("Generating {ModuleName} module", module.Name);
                    await module.GenerateAsync(context);
                }
            }

            _logger.LogInformation("Template generation completed successfully");
        }
    }
} 