using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microservice.Core.TemplateEngine;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine.Abstractions;

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
            return config.Deployment?.Docker?.Enabled == true;
        }

        public async Task GenerateAsync(GenerationContext context)
        {
            _logger.LogInformation("Generating Docker configuration");

            // Generate Dockerfile
            await GenerateDockerfileAsync(context);

            // Generate docker-compose.yml
            await GenerateDockerComposeAsync(context);

            // Generate Makefile
            await GenerateMakefileAsync(context);

            _logger.LogInformation("Docker configuration generated successfully");
        }

        private async Task GenerateDockerfileAsync(GenerationContext context)
        {
            var dockerfileContent = GenerateDockerfileContent(context);
            await context.WriteFileAsync("Dockerfile", dockerfileContent);
        }

        private async Task GenerateDockerComposeAsync(GenerationContext context)
        {
            var dockerComposeContent = GenerateDockerComposeContent(context);
            await context.WriteFileAsync("docker-compose.yml", dockerComposeContent);
        }

        private async Task GenerateMakefileAsync(GenerationContext context)
        {
            var makefileContent = GenerateMakefileContent(context);
            await context.WriteFileAsync("Makefile", makefileContent);
        }

        private string GenerateDockerfileContent(GenerationContext context)
        {
            var config = context.Configuration;
            
            // Build conditional COPY statements based on enabled modules
            var copyStatements = new List<string>();
            var restoreProjects = new List<string>();
            
            // API project is always included
            copyStatements.Add($@"COPY [""src/Api/{config.MicroserviceName}.Api.csproj"", ""src/Api/""]");
            restoreProjects.Add($@"""src/Api/{config.MicroserviceName}.Api.csproj""");
            
            // Add other projects based on configuration
            // Application layer is always included for standard and enterprise levels
            if (config.Architecture?.Level != "minimal")
            {
                copyStatements.Add($@"COPY [""src/Application/{config.MicroserviceName}.Application.csproj"", ""src/Application/""]");
            }
            
            if (config.Architecture?.Level != "minimal")
            {
                copyStatements.Add($@"COPY [""src/Domain/{config.MicroserviceName}.Domain.csproj"", ""src/Domain/""]");
                copyStatements.Add($@"COPY [""src/Infrastructure/{config.MicroserviceName}.Infrastructure.csproj"", ""src/Infrastructure/""]");
            }
            
            var copySection = string.Join("\n", copyStatements);
            var restoreCommand = string.Join(" ", restoreProjects);
            
            return $@"FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency resolution
{copySection}

# Restore dependencies
RUN dotnet restore {restoreCommand}

# Copy source code
COPY . .

# Build the application
WORKDIR ""/src/Api""
RUN dotnet build ""{config.MicroserviceName}.Api.csproj"" -c Release -o /app/build

FROM build AS publish
WORKDIR ""/src/Api""
RUN dotnet publish ""{config.MicroserviceName}.Api.csproj"" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [""dotnet"", ""{config.MicroserviceName}.Api.dll""]";
        }

        private string GenerateDockerComposeContent(GenerationContext context)
        {
            var config = context.Configuration;
            var serviceName = config.MicroserviceName.ToLower();
            return $@"version: '3.8'

services:
  {serviceName}-api:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - ""5000:80""
      - ""5001:443""
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database={serviceName};Username=postgres;Password=postgres123;
    depends_on:
      - postgres
      - redis
      - rabbitmq

  postgres:
    image: postgres:15
    environment:
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres123
      - POSTGRES_DB={serviceName}
    ports:
      - ""5432:5432""
    volumes:
      - postgres-data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - ""6379:6379""

  rabbitmq:
    image: rabbitmq:3-management
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    ports:
      - ""5672:5672""
      - ""15672:15672""

volumes:
  postgres-data:";
        }

        private string GenerateMakefileContent(GenerationContext context)
        {
            var config = context.Configuration;
            var serviceName = config.MicroserviceName.ToLower();
            return $@".PHONY: build run test clean docker-build docker-run docker-stop

# Build the application
build:
	dotnet build

# Run the application
run:
	dotnet run --project src/Api/{config.MicroserviceName}.Api.csproj

# Run tests
test:
	dotnet test

# Clean build artifacts
clean:
	dotnet clean
	rm -rf bin obj

# Docker commands
docker-build:
	docker-compose build

docker-run:
	docker-compose up -d

docker-stop:
	docker-compose down

docker-logs:
	docker-compose logs -f

# Development setup
dev-setup:
	docker-compose up -d postgres redis rabbitmq

dev-stop:
	docker-compose down

# Database migrations
migrate:
	dotnet ef database update --project src/Infrastructure/{config.MicroserviceName}.Infrastructure.csproj --startup-project src/Api/{config.MicroserviceName}.Api.csproj

# Generate migration
migration:
	@read -p ""Enter migration name: "" name; \
	dotnet ef migrations add $$name --project src/Infrastructure/{config.MicroserviceName}.Infrastructure.csproj --startup-project src/Api/{config.MicroserviceName}.Api.csproj";
        }
    }
}
 