using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microservice.Core.TemplateEngine;
using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.Api
{
    public class ApiModule : ITemplateModule
    {
        public string Name => "Api";
        public string Description => "Generates API layer with controllers, middleware, and configuration";

        public bool IsEnabled(TemplateConfiguration config)
        {
            return true; // API is always enabled
        }

        public async Task GenerateAsync(GenerationContext context)
        {
            if (!IsEnabled(context.Configuration))
                return;

            await CreateProjectStructureAsync(context);

            // Generate controllers
            foreach (var aggregate in context.Configuration.Domain?.Aggregates ?? new List<AggregateConfiguration>())
            {
                await GenerateControllerAsync(context, aggregate);
            }

            // Generate middleware
            await GenerateMiddlewareAsync(context);

            // Generate configuration
            await GenerateConfigurationAsync(context);
        }

        private async Task CreateProjectStructureAsync(GenerationContext context)
        {
            var apiPath = context.GetApiProjectPath();

            // Create project file
            await context.WriteFileAsync(
                Path.Combine("Api", $"{context.Configuration.MicroserviceName}.Api.csproj"),
                GenerateProjectFile(context.Configuration));

            // Create base directories
            Directory.CreateDirectory(Path.Combine(apiPath, "Controllers"));
            Directory.CreateDirectory(Path.Combine(apiPath, "Middleware"));
            Directory.CreateDirectory(Path.Combine(apiPath, "Configuration"));
            Directory.CreateDirectory(Path.Combine(apiPath, "Extensions"));
        }

        private async Task GenerateControllerAsync(GenerationContext context, AggregateConfiguration aggregate)
        {
            // Generate controller
            await context.WriteFileAsync(
                Path.Combine("Api", "Controllers", $"{aggregate.Name}Controller.cs"),
                GenerateController(context.Configuration, aggregate));
        }

        private async Task GenerateMiddlewareAsync(GenerationContext context)
        {
            // Generate exception handling middleware
            await context.WriteFileAsync(
                Path.Combine("Api", "Middleware", "ExceptionHandlingMiddleware.cs"),
                GenerateExceptionHandlingMiddleware(context.Configuration));

            // Generate request logging middleware
            await context.WriteFileAsync(
                Path.Combine("Api", "Middleware", "RequestLoggingMiddleware.cs"),
                GenerateRequestLoggingMiddleware(context.Configuration));

            // Generate correlation middleware
            await context.WriteFileAsync(
                Path.Combine("Api", "Middleware", "CorrelationMiddleware.cs"),
                GenerateCorrelationMiddleware(context.Configuration));
        }

        private async Task GenerateConfigurationAsync(GenerationContext context)
        {
            // Generate Program.cs
            await context.WriteFileAsync(
                Path.Combine("Api", "Program.cs"),
                GenerateProgram(context.Configuration));

            // Generate appsettings.json
            await context.WriteFileAsync(
                Path.Combine("Api", "appsettings.json"),
                GenerateAppSettings(context.Configuration));

            // Generate appsettings.Development.json
            await context.WriteFileAsync(
                Path.Combine("Api", "appsettings.Development.json"),
                GenerateDevelopmentAppSettings(context.Configuration));

            // Generate launchSettings.json
            Directory.CreateDirectory(Path.Combine(context.GetApiProjectPath(), "Properties"));
            await context.WriteFileAsync(
                Path.Combine("Api", "Properties", "launchSettings.json"),
                GenerateLaunchSettings(context.Configuration));

            // Generate API configuration
            await context.WriteFileAsync(
                Path.Combine("Api", "Configuration", "ApiConfiguration.cs"),
                GenerateApiConfiguration(context.Configuration));

            // Generate API extensions
            await context.WriteFileAsync(
                Path.Combine("Api", "Extensions", "ServiceCollectionExtensions.cs"),
                GenerateServiceCollectionExtensions(context.Configuration));
        }

        private string GenerateProjectFile(TemplateConfiguration config)
        {
            return $@"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <InvariantGlobalization>true</InvariantGlobalization>
    <AssemblyName>{config.MicroserviceName}.Api</AssemblyName>
    <RootNamespace>{config.Namespace}.Api</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNetCore.OpenApi"" Version=""8.0.5"" />
    <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""6.5.0"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""../Application/{config.MicroserviceName}.Application.csproj"" />
    <ProjectReference Include=""../Infrastructure/{config.MicroserviceName}.Infrastructure.csproj"" />
  </ItemGroup>

</Project>";
        }

        private string GenerateController(TemplateConfiguration config, AggregateConfiguration aggregate)
        {
            // Controller implementation here
            return $@"using Microsoft.AspNetCore.Mvc;
using {config.Namespace}.Application.{aggregate.Name}.Commands;
using {config.Namespace}.Application.{aggregate.Name}.Queries;

namespace {config.Namespace}.Api.Controllers
{{
    [ApiController]
    [Route(""api/[controller]"")]
    public class {aggregate.Name}Controller : ControllerBase
    {{
        // Controller implementation
    }}
}}";
        }

        private string GenerateExceptionHandlingMiddleware(TemplateConfiguration config)
        {
            // Middleware implementation here
            return $@"using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace {config.Namespace}.Api.Middleware
{{
    public class ExceptionHandlingMiddleware
    {{
        // Middleware implementation
    }}
}}";
        }

        private string GenerateRequestLoggingMiddleware(TemplateConfiguration config)
        {
            // Middleware implementation here
            return $@"using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace {config.Namespace}.Api.Middleware
{{
    public class RequestLoggingMiddleware
    {{
        // Middleware implementation
    }}
}}";
        }

        private string GenerateCorrelationMiddleware(TemplateConfiguration config)
        {
            // Middleware implementation here
            return $@"using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace {config.Namespace}.Api.Middleware
{{
    public class CorrelationMiddleware
    {{
        // Middleware implementation
    }}
}}";
        }

        private string GenerateProgram(TemplateConfiguration config)
        {
            // Program.cs implementation here
            return $@"using {config.Namespace}.Api.Extensions;
using {config.Namespace}.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application and infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{{
    app.UseSwagger();
    app.UseSwaggerUI();
}}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<CorrelationMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();";
        }

        private string GenerateAppSettings(TemplateConfiguration config)
        {
            // appsettings.json implementation here
            return $@"{{
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }}
  }},
  ""AllowedHosts"": ""*"",
  ""ConnectionStrings"": {{
    ""DefaultConnection"": """"
  }}
}}";
        }

        private string GenerateDevelopmentAppSettings(TemplateConfiguration config)
        {
            // appsettings.Development.json implementation here
            return $@"{{
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Debug"",
      ""Microsoft.AspNetCore"": ""Information""
    }}
  }},
  ""ConnectionStrings"": {{
    ""DefaultConnection"": """"
  }}
}}";
        }

        private string GenerateLaunchSettings(TemplateConfiguration config)
        {
            // launchSettings.json implementation here
            return $@"{{
  ""$schema"": ""http://json.schemastore.org/launchsettings.json"",
  ""profiles"": {{
    ""{config.MicroserviceName}.Api"": {{
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""launchBrowser"": true,
      ""launchUrl"": ""swagger"",
      ""applicationUrl"": ""https://localhost:7001;http://localhost:5001"",
      ""environmentVariables"": {{
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }}
    }}
  }}
}}";
        }

        private string GenerateApiConfiguration(TemplateConfiguration config)
        {
            // ApiConfiguration.cs implementation here
            return $@"namespace {config.Namespace}.Api.Configuration
{{
    public class ApiConfiguration
    {{
        // API configuration
    }}
}}";
        }

        private string GenerateServiceCollectionExtensions(TemplateConfiguration config)
        {
            // ServiceCollectionExtensions.cs implementation here
            return $@"using Microsoft.Extensions.DependencyInjection;

namespace {config.Namespace}.Api.Extensions
{{
    public static class ServiceCollectionExtensions
    {{
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {{
            // Register API services
            return services;
        }}
    }}
}}";
        }
    }
} 