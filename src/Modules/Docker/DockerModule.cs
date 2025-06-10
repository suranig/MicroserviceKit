using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microservice.Core.TemplateEngine;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.Docker
{
    public class DockerModule : ITemplateModule
    {
        private readonly ILogger<DockerModule> _logger;

        public DockerModule(ILogger<DockerModule> logger)
        {
            _logger = logger;
        }

        public string Name => "Docker";
        public string Description => "Generates Docker configuration for the microservice";

        public bool IsEnabled(TemplateConfiguration config)
        {
            return config.Features?.Docker?.Enabled == true;
        }

        public async Task GenerateAsync(GenerationContext context)
        {
            _logger.LogInformation("Generating Docker configuration");

            // Create Docker directory
            var dockerPath = context.GetDockerProjectPath();
            Directory.CreateDirectory(dockerPath);

            // Generate Dockerfile
            await GenerateDockerfileAsync(context);

            // Generate docker-compose.yml
            await GenerateDockerComposeAsync(context);

            _logger.LogInformation("Docker configuration generated successfully");
        }

        private async Task GenerateDockerfileAsync(GenerationContext context)
        {
            var dockerfilePath = Path.Combine(context.GetDockerProjectPath(), "Dockerfile");
            var dockerfileContent = GenerateDockerfileContent(context);
            await context.WriteFileAsync(dockerfilePath, dockerfileContent);
        }

        private async Task GenerateDockerComposeAsync(GenerationContext context)
        {
            var dockerComposePath = Path.Combine(context.GetDockerProjectPath(), "docker-compose.yml");
            var dockerComposeContent = GenerateDockerComposeContent(context);
            await context.WriteFileAsync(dockerComposePath, dockerComposeContent);
        }

        private string GenerateDockerfileContent(GenerationContext context)
        {
            return $@"FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY [""{context.GetSourceDirectory()}/Api/{context.GetMicroserviceName()}.Api.csproj"", ""{context.GetSourceDirectory()}/Api/""]
COPY [""{context.GetSourceDirectory()}/Application/{context.GetMicroserviceName()}.Application.csproj"", ""{context.GetSourceDirectory()}/Application/""]
COPY [""{context.GetSourceDirectory()}/Domain/{context.GetMicroserviceName()}.Domain.csproj"", ""{context.GetSourceDirectory()}/Domain/""]
COPY [""{context.GetSourceDirectory()}/Infrastructure/{context.GetMicroserviceName()}.Infrastructure.csproj"", ""{context.GetSourceDirectory()}/Infrastructure/""]
RUN dotnet restore ""{context.GetSourceDirectory()}/Api/{context.GetMicroserviceName()}.Api.csproj""
COPY . .
WORKDIR ""/src/{context.GetSourceDirectory()}/Api""
RUN dotnet build ""{context.GetMicroserviceName()}.Api.csproj"" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish ""{context.GetMicroserviceName()}.Api.csproj"" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [""dotnet"", ""{context.GetMicroserviceName()}.Api.dll""]";
        }

        private string GenerateDockerComposeContent(GenerationContext context)
        {
            return $@"version: '3.4'

services:
  {context.GetMicroserviceName().ToLower()}.api:
    image: {context.GetMicroserviceName().ToLower()}/api
    build:
      context: ..
      dockerfile: docker/Dockerfile
    ports:
      - ""5000:80""
      - ""5001:443""
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database={context.GetMicroserviceName().ToLower()};Username=postgres;Password=postgres;
    depends_on:
      - postgres

  postgres:
    image: postgres:latest
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
      - POSTGRES_DB={context.GetMicroserviceName().ToLower()}
    ports:
      - ""5432:5432""
    volumes:
      - postgres-data:/var/lib/postgresql/data

volumes:
  postgres-data:";
        }
    }
}
 