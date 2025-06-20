using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Logging;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine.Abstractions;

namespace Microservice.Core.TemplateEngine
{
    public class MicroserviceGenerator
    {
        private readonly GenerationContext _context;
        private readonly TemplateConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IEnumerable<ITemplateModule> _modules;

        public MicroserviceGenerator(
            GenerationContext context,
            TemplateConfiguration configuration,
            ILogger logger,
            IEnumerable<ITemplateModule> modules)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _modules = modules;
        }

        public async Task GenerateAsync()
        {
            _logger.LogInformation("Starting microservice generation for {MicroserviceName}", _configuration.MicroserviceName);

            // Create solution file
            await CreateSolutionFileAsync();

            // Generate modules
            foreach (var module in _modules)
            {
                if (module.IsEnabled(_configuration))
                {
                    _logger.LogInformation("Generating {ModuleName} module", module.Name);
                    await module.GenerateAsync(_context);
                }
            }

            _logger.LogInformation("Microservice generation completed successfully");
        }

        private async Task CreateSolutionFileAsync()
        {
            var solutionPath = Path.Combine(_configuration.OutputPath, $"{_configuration.MicroserviceName}.sln");
            var solutionContent = GenerateSolutionFile();
            
            await _context.WriteFileAsync($"{_configuration.MicroserviceName}.sln", solutionContent);
        }

        private string GenerateSolutionFile()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            sb.AppendLine("# Visual Studio Version 17");
            sb.AppendLine("VisualStudioVersion = 17.0.31903.59");
            sb.AppendLine("MinimumVisualStudioVersion = 10.0.40219.1");

            // Add projects with consistent GUIDs
            var sourceDir = _configuration.ProjectStructure?.SourceDirectory ?? "src";
            var projects = new[]
            {
                new { Name = "Domain", Path = Path.Combine(sourceDir, "Domain", $"{_configuration.MicroserviceName}.Domain.csproj"), Guid = Guid.NewGuid().ToString("B").ToUpper() },
                new { Name = "Application", Path = Path.Combine(sourceDir, "Application", $"{_configuration.MicroserviceName}.Application.csproj"), Guid = Guid.NewGuid().ToString("B").ToUpper() },
                new { Name = "Infrastructure", Path = Path.Combine(sourceDir, "Infrastructure", $"{_configuration.MicroserviceName}.Infrastructure.csproj"), Guid = Guid.NewGuid().ToString("B").ToUpper() },
                new { Name = "Api", Path = Path.Combine(sourceDir, "Api", $"{_configuration.MicroserviceName}.Api.csproj"), Guid = Guid.NewGuid().ToString("B").ToUpper() },
                new { Name = "Tests", Path = Path.Combine("tests", $"{_configuration.MicroserviceName}.Tests.csproj"), Guid = Guid.NewGuid().ToString("B").ToUpper() }
            };

            foreach (var project in projects)
            {
                sb.AppendLine($"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{project.Name}\", \"{project.Path}\", \"{project.Guid}\"");
                sb.AppendLine("EndProject");
            }

            // Add solution folders
            sb.AppendLine("Global");
            sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            sb.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
            sb.AppendLine("\t\tRelease|Any CPU = Release|Any CPU");
            sb.AppendLine("\tEndGlobalSection");

            sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var project in projects)
            {
                sb.AppendLine($"\t\t{project.Guid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                sb.AppendLine($"\t\t{project.Guid}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                sb.AppendLine($"\t\t{project.Guid}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                sb.AppendLine($"\t\t{project.Guid}.Release|Any CPU.Build.0 = Release|Any CPU");
            }
            sb.AppendLine("\tEndGlobalSection");

            sb.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
            sb.AppendLine("\t\tHideSolutionNode = FALSE");
            sb.AppendLine("\tEndGlobalSection");

            sb.AppendLine("EndGlobal");

            return sb.ToString();
        }
    }
} 