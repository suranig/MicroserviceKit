# Microservice Template Generator - Plan Implementacji

## Przegląd Projektu

**Cel:** Stworzenie konfigurowalnego template generatora dla mikroserwisów .NET 8 z podejściem modularnym "klocki Lego", który pozwala na składanie mikroserwisów według wybranych kryteriów.

**Filozofia:** Open Source, modularność, konfigurowalność, najlepsze praktyki DDD/Clean Architecture.

## Analiza Obecnego Stanu

### Co już mamy:
- ✅ Podstawowa struktura Clean Architecture (Domain, Application, Infrastructure, API)
- ✅ Implementacja CQRS z MediatR
- ✅ Podstawowe Todo entity z repository pattern
- ✅ Minimal API endpoints
- ✅ Testy jednostkowe (struktura)

### Co wymaga refaktoryzacji:
- ❌ MediatR (przechodzi na licencję komercyjną) → zamiana na open source alternatywę
- ❌ Brak DDD patterns (Aggregates, Value Objects, Domain Events)
- ❌ Brak konfigurowalności i modularności
- ❌ Brak template generation capabilities
- ❌ Brak Docker/Kubernetes support
- ❌ Brak messaging/event sourcing
- ❌ Brak background workers

## Architektura Template Generatora

### 1. Core Template Engine
```
MicroserviceTemplateGenerator/
├── src/
│   ├── Core/
│   │   ├── TemplateEngine/           # Silnik generowania
│   │   ├── Configuration/            # Konfiguracja modułów
│   │   ├── Abstractions/            # Interfejsy dla modułów
│   │   └── Common/                  # Wspólne komponenty
│   ├── Modules/                     # Moduły "klocki Lego"
│   │   ├── DDD/                     # Domain-Driven Design
│   │   ├── CQRS/                    # Command Query Responsibility Segregation
│   │   ├── EventSourcing/           # Event Sourcing
│   │   ├── Messaging/               # RabbitMQ, Azure Service Bus
│   │   ├── Persistence/             # MongoDB, PostgreSQL, SQL Server
│   │   ├── API/                     # REST, gRPC, GraphQL
│   │   ├── BackgroundWorkers/       # Background processing
│   │   ├── Authentication/          # JWT, OAuth2
│   │   ├── Monitoring/              # Logging, Metrics, Health Checks
│   │   └── Containerization/        # Docker, Kubernetes
│   ├── CLI/                         # Command Line Interface
│   └── Templates/                   # Szablony kodu
└── tests/
```

### 2. Moduły "Klocki Lego"

#### A. DDD Module
- Aggregate Root base classes
- Value Objects
- Domain Events
- Domain Services
- Repository patterns
- Specifications

#### B. CQRS Module  
- Command/Query interfaces
- Handler base classes
- Open source mediator (Wolverine lub własna implementacja)
- Pipeline behaviors

#### C. Event Sourcing Module
- Event Store abstractions
- Event sourcing base classes
- Projections
- Snapshots

#### D. Messaging Module
- RabbitMQ integration
- Azure Service Bus
- In-memory messaging
- Outbox pattern

#### E. Persistence Module
- MongoDB support
- PostgreSQL support
- SQL Server support
- Read/Write model separation

#### F. API Module
- REST API with controllers
- Minimal APIs
- gRPC services
- GraphQL (Hot Chocolate)

#### G. Background Workers Module
- Hangfire integration
- Quartz.NET integration
- Custom background services

#### H. Containerization Module
- Dockerfile generation
- Docker Compose
- Kubernetes manifests
- Helm charts

## Faza 1: Refaktoryzacja Obecnego Kodu

### 1.1 Zamiana MediatR na Open Source Alternative

**Wybrana alternatywa: Wolverine**
- MIT License
- Wysoką wydajność (source generators)
- Built-in messaging
- Saga support
- Background jobs

**Kroki:**
1. Usunięcie MediatR
2. Instalacja Wolverine
3. Refaktoryzacja handlerów
4. Aktualizacja DI configuration

### 1.2 Implementacja DDD Patterns

**Wykorzystanie AggregateKit (Twój NuGet):**
```csharp
// Domain/Common/AggregateRoot.cs
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

### 1.3 Implementacja Value Objects
```csharp
// Domain/ValueObjects/Email.cs
public record Email
{
    public string Value { get; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");
            
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format");
            
        Value = value;
    }
    
    private static bool IsValidEmail(string email) => 
        new EmailAddressAttribute().IsValid(email);
}
```

## Faza 2: Template Engine Implementation

### 2.1 Configuration System

**appsettings.template.json:**
```json
{
  "microservice": {
    "name": "OrderService",
    "namespace": "Company.OrderService",
    "modules": {
      "ddd": {
        "enabled": true,
        "aggregates": ["Order", "Customer"],
        "valueObjects": ["Money", "Address"]
      },
      "cqrs": {
        "enabled": true,
        "mediator": "wolverine"
      },
      "eventSourcing": {
        "enabled": false
      },
      "messaging": {
        "enabled": true,
        "provider": "rabbitmq",
        "exchanges": ["orders", "customers"]
      },
      "persistence": {
        "writeModel": "postgresql",
        "readModel": "mongodb"
      },
      "api": {
        "types": ["rest", "grpc"],
        "authentication": "jwt"
      },
      "backgroundWorkers": {
        "enabled": true,
        "provider": "hangfire"
      },
      "containerization": {
        "docker": true,
        "kubernetes": true
      }
    }
  }
}
```

### 2.2 CLI Interface

**Commands:**
```bash
# Generowanie nowego mikroserwisu
dotnet-microservice new OrderService --config appsettings.template.json

# Dodawanie nowego agregatu
dotnet-microservice add aggregate Order --properties "CustomerId:Guid,TotalAmount:decimal"

# Dodawanie API endpoint
dotnet-microservice add api orders --type rest --operations "create,read,update,delete"

# Dodawanie gRPC service
dotnet-microservice add grpc OrderService --operations "CreateOrder,GetOrder"

# Generowanie Docker/K8s
dotnet-microservice add container --type docker
dotnet-microservice add container --type kubernetes
```

### 2.3 Template Engine Core

```csharp
public interface ITemplateEngine
{
    Task<GenerationResult> GenerateAsync(TemplateConfiguration config);
}

public class TemplateEngine : ITemplateEngine
{
    private readonly IEnumerable<ITemplateModule> _modules;
    private readonly IFileSystem _fileSystem;
    
    public async Task<GenerationResult> GenerateAsync(TemplateConfiguration config)
    {
        var context = new GenerationContext(config);
        
        foreach (var module in _modules.Where(m => m.IsEnabled(config)))
        {
            await module.GenerateAsync(context);
        }
        
        return new GenerationResult(context.GeneratedFiles);
    }
}
```

## Faza 3: Implementacja Modułów

### 3.1 DDD Module

**Aggregate Generator:**
```csharp
public class AggregateGenerator : ICodeGenerator
{
    public async Task GenerateAsync(GenerationContext context)
    {
        foreach (var aggregate in context.Config.DDD.Aggregates)
        {
            var template = await LoadTemplate("Aggregate.cs.template");
            var code = template
                .Replace("{{AggregateeName}}", aggregate.Name)
                .Replace("{{Properties}}", GenerateProperties(aggregate.Properties))
                .Replace("{{Methods}}", GenerateMethods(aggregate.Methods));
                
            await context.WriteFileAsync($"Domain/Aggregates/{aggregate.Name}.cs", code);
        }
    }
}
```

**Template Aggregate.cs.template:**
```csharp
using {{Namespace}}.Domain.Common;
using {{Namespace}}.Domain.Events;

namespace {{Namespace}}.Domain.Aggregates;

public class {{AggregateName}} : AggregateRoot<{{AggregateName}}Id>
{
    {{Properties}}
    
    private {{AggregateName}}() { } // EF Core
    
    public static {{AggregateName}} Create({{CreateParameters}})
    {
        var aggregate = new {{AggregateName}}
        {
            {{PropertyAssignments}}
        };
        
        aggregate.AddDomainEvent(new {{AggregateName}}CreatedEvent(aggregate.Id));
        return aggregate;
    }
    
    {{Methods}}
}
```

### 3.2 CQRS Module z Wolverine

**Command Generator:**
```csharp
// Templates/Command.cs.template
namespace {{Namespace}}.Application.{{AggregateName}}.Commands;

public record {{CommandName}}Command({{Parameters}});

public class {{CommandName}}Handler
{
    private readonly I{{AggregateName}}Repository _repository;
    
    public {{CommandName}}Handler(I{{AggregateName}}Repository repository)
    {
        _repository = repository;
    }
    
    public async Task<{{ReturnType}}> Handle({{CommandName}}Command command)
    {
        {{HandlerLogic}}
    }
}
```

### 3.3 API Module

**REST Controller Generator:**
```csharp
// Templates/Controller.cs.template
[ApiController]
[Route("api/[controller]")]
public class {{AggregateName}}Controller : ControllerBase
{
    private readonly IMessageBus _messageBus;
    
    public {{AggregateName}}Controller(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }
    
    {{Endpoints}}
}
```

**gRPC Service Generator:**
```csharp
// Templates/GrpcService.cs.template
public class {{ServiceName}} : {{ServiceName}}Base
{
    private readonly IMessageBus _messageBus;
    
    public {{ServiceName}}(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }
    
    {{Methods}}
}
```

### 3.4 Persistence Module

**Repository Generator:**
```csharp
// Templates/Repository.cs.template
public class {{AggregateName}}Repository : I{{AggregateName}}Repository
{
    private readonly {{DbContextName}} _context;
    
    public {{AggregateName}}Repository({{DbContextName}} context)
    {
        _context = context;
    }
    
    {{RepositoryMethods}}
}
```

### 3.5 Messaging Module

**RabbitMQ Configuration:**
```csharp
// Templates/RabbitMqConfiguration.cs.template
public static class RabbitMqConfiguration
{
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(
            configuration.GetSection("RabbitMq"));
            
        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
        services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
        
        return services;
    }
}
```

### 3.6 Background Workers Module

**Hangfire Worker Generator:**
```csharp
// Templates/BackgroundWorker.cs.template
public class {{WorkerName}} : IBackgroundWorker
{
    private readonly ILogger<{{WorkerName}}> _logger;
    
    public {{WorkerName}}(ILogger<{{WorkerName}}> logger)
    {
        _logger = logger;
    }
    
    [RecurringJob("{{JobId}}", "{{CronExpression}}")]
    public async Task ExecuteAsync()
    {
        {{WorkerLogic}}
    }
}
```

## Faza 4: Containerization & Orchestration

### 4.1 Docker Support

**Dockerfile Generator:**
```dockerfile
# Templates/Dockerfile.template
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

{{CopyProjects}}

RUN dotnet restore "{{MainProject}}"
COPY . .
WORKDIR "/src/{{MainProjectPath}}"
RUN dotnet build "{{MainProject}}" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "{{MainProject}}" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "{{MainAssembly}}.dll"]
```

**Docker Compose Generator:**
```yaml
# Templates/docker-compose.yml.template
version: '3.8'

services:
  {{ServiceName}}:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "{{Port}}:80"
    environment:
      {{EnvironmentVariables}}
    depends_on:
      {{Dependencies}}

  {{DatabaseServices}}
  
  {{MessageBrokerServices}}
```

### 4.2 Kubernetes Support

**Deployment Generator:**
```yaml
# Templates/k8s-deployment.yml.template
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ServiceName}}
  labels:
    app: {{ServiceName}}
spec:
  replicas: {{Replicas}}
  selector:
    matchLabels:
      app: {{ServiceName}}
  template:
    metadata:
      labels:
        app: {{ServiceName}}
    spec:
      containers:
      - name: {{ServiceName}}
        image: {{ImageName}}:{{Tag}}
        ports:
        - containerPort: 80
        env:
        {{EnvironmentVariables}}
```

## Faza 5: CLI Implementation

### 5.1 CLI Structure

```csharp
// CLI/Commands/NewCommand.cs
[Command("new")]
public class NewCommand : AsyncCommand<NewCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<name>")]
        public string Name { get; set; } = string.Empty;
        
        [CommandOption("--config")]
        public string? ConfigFile { get; set; }
        
        [CommandOption("--output")]
        public string OutputPath { get; set; } = "./";
    }
    
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var config = await LoadConfiguration(settings.ConfigFile);
        var engine = new TemplateEngine();
        
        await engine.GenerateAsync(config);
        
        return 0;
    }
}
```

### 5.2 Interactive Configuration

```csharp
// CLI/Interactive/ConfigurationWizard.cs
public class ConfigurationWizard
{
    public async Task<TemplateConfiguration> RunAsync()
    {
        var config = new TemplateConfiguration();
        
        config.Name = AnsiConsole.Ask<string>("Microservice name:");
        config.Namespace = AnsiConsole.Ask<string>("Namespace:", $"Company.{config.Name}");
        
        // DDD Module
        if (AnsiConsole.Confirm("Enable DDD patterns?"))
        {
            config.DDD.Enabled = true;
            config.DDD.Aggregates = await ConfigureAggregates();
        }
        
        // CQRS Module
        if (AnsiConsole.Confirm("Enable CQRS?"))
        {
            config.CQRS.Enabled = true;
            config.CQRS.Mediator = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Choose mediator:")
                    .AddChoices("wolverine", "custom"));
        }
        
        // Continue for other modules...
        
        return config;
    }
}
```

## Faza 6: Testing Strategy

### 6.1 Template Testing

```csharp
// Tests/TemplateEngine/AggregateGeneratorTests.cs
public class AggregateGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_ShouldCreateValidAggregateClass()
    {
        // Arrange
        var config = new TemplateConfiguration
        {
            DDD = new DDDConfiguration
            {
                Aggregates = new[]
                {
                    new AggregateConfiguration
                    {
                        Name = "Order",
                        Properties = new[] { "CustomerId:Guid", "TotalAmount:decimal" }
                    }
                }
            }
        };
        
        var generator = new AggregateGenerator();
        var context = new GenerationContext(config);
        
        // Act
        await generator.GenerateAsync(context);
        
        // Assert
        var generatedFile = context.GeneratedFiles
            .FirstOrDefault(f => f.Path.EndsWith("Order.cs"));
            
        generatedFile.Should().NotBeNull();
        generatedFile.Content.Should().Contain("public class Order : AggregateRoot<OrderId>");
    }
}
```

### 6.2 Integration Testing

```csharp
// Tests/Integration/FullGenerationTests.cs
public class FullGenerationTests
{
    [Fact]
    public async Task GenerateFullMicroservice_ShouldCompileSuccessfully()
    {
        // Arrange
        var config = LoadTestConfiguration();
        var engine = new TemplateEngine();
        
        // Act
        var result = await engine.GenerateAsync(config);
        
        // Assert
        var compilation = await CompileGeneratedCode(result.GeneratedFiles);
        compilation.Success.Should().BeTrue();
    }
}
```

## Faza 7: Documentation & Examples

### 7.1 README Generation

```markdown
# {{MicroserviceName}}

Generated microservice using MicroserviceTemplateGenerator.

## Architecture

This microservice implements:
{{#if DDD.Enabled}}
- Domain-Driven Design patterns
{{/if}}
{{#if CQRS.Enabled}}
- CQRS with {{CQRS.Mediator}}
{{/if}}
{{#if EventSourcing.Enabled}}
- Event Sourcing
{{/if}}

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Docker (optional)

### Running locally
```bash
dotnet run --project src/{{MicroserviceName}}.Api
```

### Running with Docker
```bash
docker-compose up
```

## API Documentation

{{#each API.Endpoints}}
### {{Method}} {{Path}}
{{Description}}
{{/each}}
```

### 7.2 Example Configurations

**E-commerce Order Service:**
```json
{
  "microservice": {
    "name": "OrderService",
    "namespace": "ECommerce.OrderService",
    "modules": {
      "ddd": {
        "enabled": true,
        "aggregates": [
          {
            "name": "Order",
            "properties": ["CustomerId:Guid", "TotalAmount:Money", "Status:OrderStatus"],
            "methods": ["AddItem", "RemoveItem", "Confirm", "Cancel"]
          }
        ],
        "valueObjects": ["Money", "Address"]
      },
      "cqrs": {
        "enabled": true,
        "mediator": "wolverine"
      },
      "messaging": {
        "enabled": true,
        "provider": "rabbitmq"
      },
      "persistence": {
        "writeModel": "postgresql",
        "readModel": "mongodb"
      },
      "api": {
        "types": ["rest", "grpc"]
      }
    }
  }
}
```

## Faza 8: Deployment & Distribution

### 8.1 NuGet Package

```xml
<!-- MicroserviceTemplateGenerator.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>dotnet-microservice</ToolCommandName>
    <PackageId>MicroserviceTemplateGenerator</PackageId>
    <Version>1.0.0</Version>
    <Authors>Your Name</Authors>
    <Description>Configurable template generator for .NET 8 microservices</Description>
    <PackageTags>microservices;template;generator;ddd;cqrs;dotnet</PackageTags>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
  </PropertyGroup>
</Project>
```

### 8.2 Installation

```bash
# Install as global tool
dotnet tool install -g MicroserviceTemplateGenerator

# Use
dotnet microservice new OrderService
```

## Roadmap

### Phase 1 (MVP) - 4 tygodnie
- [ ] Refaktoryzacja obecnego kodu (Wolverine, DDD patterns)
- [ ] Podstawowy template engine
- [ ] CLI z podstawowymi komendami
- [ ] DDD i CQRS moduły

### Phase 2 - 3 tygodnie  
- [ ] API moduły (REST, gRPC)
- [ ] Persistence moduł
- [ ] Messaging moduł
- [ ] Docker support

### Phase 3 - 2 tygodnie
- [ ] Background workers
- [ ] Kubernetes support
- [ ] Testing framework
- [ ] Documentation generation

### Phase 4 - 1 tydzień
- [ ] NuGet packaging
- [ ] CI/CD pipeline
- [ ] Examples i dokumentacja

## Open Source Dependencies

**Zamiast komercyjnych:**
- ✅ **Wolverine** zamiast MediatR (MIT License)
- ✅ **Mapperly** zamiast AutoMapper (Apache 2.0)
- ✅ **FluentValidation** (Apache 2.0) - nadal darmowy
- ✅ **Serilog** zamiast NLog (Apache 2.0)

**Messaging:**
- ✅ **RabbitMQ.Client** (Apache 2.0/MPL 2.0)
- ✅ **MassTransit** v8.x (Apache 2.0) - ostatnia darmowa wersja

**Testing:**
- ✅ **xUnit** (Apache 2.0)
- ✅ **FluentAssertions** v6.x (Apache 2.0) - ostatnia darmowa
- ✅ **NSubstitute** zamiast Moq (BSD)

## Metodologia Rozwoju

### 1. Test-Driven Development
- Każdy moduł ma testy jednostkowe
- Integration testy dla pełnego flow
- Template validation tests

### 2. Continuous Integration
```yaml
# .github/workflows/ci.yml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - run: dotnet test
    - run: dotnet pack
```

### 3. Semantic Versioning
- Major: Breaking changes w API
- Minor: Nowe moduły/features
- Patch: Bug fixes

### 4. Documentation-First
- Każdy moduł ma README
- API documentation
- Examples dla każdego modułu

## Status Implementacji - Checklist

### ✅ ZROBIONE

#### Faza 1: Refaktoryzacja Podstaw
- [x] Stworzenie branch `template-generator`
- [x] Refaktoryzacja MediatR → Wolverine (MIT License)
- [x] Integracja z AggregateKit (Twoja biblioteka DDD)
- [x] Aktualizacja TodoItem do używania AggregateKit.AggregateRoot
- [x] Usunięcie własnych implementacji DDD (używamy AggregateKit)

#### Faza 2: Template Engine - Podstawy
- [x] Struktura konfiguracji (TemplateConfiguration.cs)
- [x] Interfejs modułów (ITemplateModule.cs)
- [x] DDDModule dla generowania agregatów
- [x] Przykładowa konfiguracja JSON (template-config.example.json)

#### Faza 3: CLI Tool
- [x] Program.cs z System.CommandLine
- [x] Interactive mode (jak cookiecutter)
- [x] Obsługa plików konfiguracyjnych
- [x] Domyślny template
- [x] Dokumentacja użycia (USAGE.md)

### ❌ DO NAPRAWY (KRYTYCZNE)

#### Problemy kompilacji
- [ ] **BŁĄD**: Brakuje plików .csproj dla modułów:
  - [ ] `src/Core/TemplateEngine/Configuration/TemplateConfiguration.csproj`
  - [ ] `src/Core/TemplateEngine/Abstractions/ITemplateModule.csproj`
  - [ ] `src/Modules/DDD/DDDModule.csproj`
- [ ] **BŁĄD**: Błąd składni w Program.cs (linia 245)
- [ ] **BŁĄD**: CLI nie może się skompilować

#### Struktura projektów
- [ ] Dodanie projektów do solution
- [ ] Poprawne referencje między projektami
- [ ] Testowanie kompilacji

### 🚧 DO ZROBIENIA (NASTĘPNE KROKI)

#### Faza 4: Dokończenie Core Modules
- [ ] Stworzenie brakujących plików .csproj
- [ ] Naprawienie błędów kompilacji
- [ ] Testowanie CLI tool
- [ ] Generowanie pierwszego mikroserwisu

#### Faza 5: API Module
- [ ] Moduł generowania REST API
- [ ] Generowanie kontrolerów
- [ ] Generowanie Minimal API endpoints
- [ ] Integracja z Wolverine

#### Faza 6: CQRS Module
- [ ] Generowanie Commands/Queries
- [ ] Generowanie Handlerów
- [ ] Integracja z Wolverine
- [ ] Walidacja z FluentValidation

#### Faza 7: Infrastructure Module
- [ ] Repository pattern
- [ ] Unit of Work
- [ ] Konfiguracja DI
- [ ] Persistence providers (InMemory, PostgreSQL, SQL Server)

#### Faza 8: Project Structure Module
- [ ] Generowanie .csproj files
- [ ] Generowanie solution file
- [ ] Generowanie Program.cs
- [ ] Generowanie appsettings.json

#### Faza 9: Containerization Module
- [ ] Dockerfile generation
- [ ] Docker Compose
- [ ] Kubernetes manifests
- [ ] Health checks

#### Faza 10: Testing & Packaging
- [ ] Unit testy dla template engine
- [ ] Integration testy
- [ ] Pakowanie jako NuGet global tool
- [ ] CI/CD pipeline

### 🎯 PRIORYTET 1 (Następne 2h)

1. **Naprawienie kompilacji**:
   ```bash
   # Utworzenie brakujących .csproj
   # Naprawienie błędów składni
   # Testowanie CLI
   ```

2. **Pierwszy working prototype**:
   ```bash
   dotnet microservice new TestService --interactive
   # Powinno wygenerować działający mikroserwis
   ```

### 🎯 PRIORYTET 2 (Następny tydzień)

1. **API Module** - generowanie REST endpoints
2. **CQRS Module** - pełna integracja z Wolverine
3. **Infrastructure Module** - repositories i DI
4. **Project Structure** - kompletne .csproj i solution

### 🎯 PRIORYTET 3 (Następne 2 tygodnie)

1. **Containerization** - Docker/K8s support
2. **Testing** - unit i integration tests
3. **Packaging** - NuGet global tool
4. **Documentation** - kompletna dokumentacja

## Następne Kroki - AKCJE

### TERAZ (następne 30 min):
1. Utworzenie brakujących plików .csproj
2. Naprawienie błędów składni w Program.cs
3. Testowanie kompilacji CLI

### DZISIAJ:
1. Pierwszy working prototype CLI
2. Generowanie prostego mikroserwisu
3. Testowanie end-to-end

### TEN TYDZIEŃ:
1. API Module implementation
2. CQRS Module completion
3. Infrastructure Module basics 