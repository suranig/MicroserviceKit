using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;
using System.Text;
using MassTransit;

namespace Microservice.Modules.Application;

public class ApplicationModule : ITemplateModule
{
    public string Name => "Application";
    public string Description => "Generates Application layer with CQRS handlers, validators, and behaviors";

    public bool IsEnabled(TemplateConfiguration config)
    {
        var decisions = ArchitectureRules.MakeDecisions(config);
        return decisions.EnableCQRS && decisions.ArchitectureLevel != ArchitectureLevel.Minimal;
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var outputPath = context.GetApplicationProjectPath();

        // Create project structure
        await CreateProjectStructureAsync(context, config);

        // Generate handlers for each aggregate
        if (config.Domain?.Aggregates != null)
        {
            foreach (var aggregate in config.Domain.Aggregates)
            {
                await GenerateAggregateHandlersAsync(context, config, aggregate);
            }
        }

        // Generate common application services
        await GenerateApplicationServicesAsync(context, config);
    }

    private async Task CreateProjectStructureAsync(GenerationContext context, TemplateConfiguration config)
    {
        // Note: context.WriteFileAsync automatically creates directories, so we don't need manual Directory.CreateDirectory
        
        // Generate .csproj file using relative path
        var csprojContent = GenerateProjectFile(config);
        await context.WriteFileAsync($"src/Application/{config.MicroserviceName}.Application.csproj", csprojContent);
    }

    private async Task GenerateAggregateHandlersAsync(GenerationContext context, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Generate Commands
        await GenerateCommandsAsync(context, config, aggregate);

        // Generate Queries  
        await GenerateQueriesAsync(context, config, aggregate);

        // Generate DTOs
        await GenerateDTOsAsync(context, config, aggregate);
    }

    private async Task GenerateCommandsAsync(GenerationContext context, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Generate CRUD commands
        var crudOperations = new[] { "Create", "Update", "Delete" };
        var customOperations = aggregate.Operations ?? new List<string>();
        var allOperations = crudOperations.Concat(customOperations).Distinct();

        foreach (var operation in allOperations)
        {
            // Generate Command
            var commandContent = GenerateCommand(config, aggregate, operation);
            await context.WriteFileAsync(
                $"src/Application/{aggregate.Name}/Commands/{operation}{aggregate.Name}/{operation}{aggregate.Name}Command.cs", 
                commandContent);

            // Generate Handler
            var handlerContent = GenerateCommandHandler(config, aggregate, operation);
            await context.WriteFileAsync(
                $"src/Application/{aggregate.Name}/Commands/{operation}{aggregate.Name}/{operation}{aggregate.Name}CommandHandler.cs", 
                handlerContent);

            // Generate Validator
            var validatorContent = GenerateCommandValidator(config, aggregate, operation);
            await context.WriteFileAsync(
                $"src/Application/{aggregate.Name}/Commands/{operation}{aggregate.Name}/{operation}{aggregate.Name}CommandValidator.cs", 
                validatorContent);
        }
    }

    private async Task GenerateQueriesAsync(GenerationContext context, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Generate standard queries
        var queries = new[]
        {
            $"Get{aggregate.Name}ById",
            $"Get{aggregate.Name}s",
            $"Get{aggregate.Name}sWithPaging"
        };

        foreach (var queryName in queries)
        {
            // Generate Query
            var queryContent = GenerateQuery(config, aggregate, queryName);
            await context.WriteFileAsync(
                $"src/Application/{aggregate.Name}/Queries/{queryName}/{queryName}Query.cs", 
                queryContent);

            // Generate Handler
            var handlerContent = GenerateQueryHandler(config, aggregate, queryName);
            await context.WriteFileAsync(
                $"src/Application/{aggregate.Name}/Queries/{queryName}/{queryName}QueryHandler.cs", 
                handlerContent);
        }
    }

    private async Task GenerateDTOsAsync(GenerationContext context, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Generate DTOs
        var dtoContent = GenerateDTO(config, aggregate);
        await context.WriteFileAsync(
            $"src/Application/{aggregate.Name}/DTOs/{aggregate.Name}Dto.cs", 
            dtoContent);

        var createDtoContent = GenerateCreateDTO(config, aggregate);
        await context.WriteFileAsync(
            $"src/Application/{aggregate.Name}/DTOs/Create{aggregate.Name}Dto.cs", 
            createDtoContent);

        var updateDtoContent = GenerateUpdateDTO(config, aggregate);
        await context.WriteFileAsync(
            $"src/Application/{aggregate.Name}/DTOs/Update{aggregate.Name}Dto.cs", 
            updateDtoContent);
    }

    private async Task GenerateApplicationServicesAsync(GenerationContext context, TemplateConfiguration config)
    {
        // Generate ServiceCollection extensions
        await context.WriteFileAsync(
            "src/Application/Extensions/ServiceCollectionExtensions.cs",
            GenerateServiceCollectionExtensions(config));

        // Generate Repository interface
        await context.WriteFileAsync(
            "src/Application/Common/IRepository.cs",
            GenerateRepositoryInterface(config));

        // Generate MassTransit middleware
        await context.WriteFileAsync(
            "src/Application/Middleware/ValidationFilter.cs",
            GenerateValidationMiddleware(config));

        await context.WriteFileAsync(
            "src/Application/Middleware/LoggingFilter.cs",
            GenerateLoggingMiddleware(config));

        // Generate common exceptions
        await context.WriteFileAsync(
            "src/Application/Common/NotFoundException.cs",
            GenerateNotFoundException(config));
    }

    private string GenerateProjectFile(TemplateConfiguration config)
    {
        return $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""FluentValidation"" Version=""11.9.0"" />
    <PackageReference Include=""FluentValidation.DependencyInjectionExtensions"" Version=""11.9.0"" />
    <PackageReference Include=""MassTransit"" Version=""8.1.3"" />
    <PackageReference Include=""MassTransit.RabbitMQ"" Version=""8.1.3"" />
    <PackageReference Include=""Microsoft.Extensions.DependencyInjection.Abstractions"" Version=""8.0.2"" />
    <PackageReference Include=""Microsoft.Extensions.Logging.Abstractions"" Version=""8.0.2"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""../Domain/{config.MicroserviceName}.Domain.csproj"" />
  </ItemGroup>

</Project>";
    }

    private string GenerateCommand(TemplateConfiguration config, AggregateConfiguration aggregate, string operation)
    {
        var parameters = operation.ToLowerInvariant() switch
        {
            "create" => GenerateCreateCommandParameters(aggregate),
            "update" => GenerateUpdateCommandParameters(aggregate),
            "delete" => "Guid id",
            _ => GenerateDefaultCommandParameters(aggregate)
        };

        var hasEnums = config.Domain?.Enums?.Any() == true;
        var enumUsing = hasEnums ? $"using {config.Namespace}.Domain.Enums;\n" : "";

        return $@"{enumUsing}namespace {config.Namespace}.Application.{aggregate.Name}.Commands.{operation}{aggregate.Name};

public record {operation}{aggregate.Name}Command({parameters});";
    }

    private string GenerateCreateCommandParameters(AggregateConfiguration aggregate)
    {
        // Filter out Id, CreatedAt, UpdatedAt for create command
        var filteredProps = aggregate.Properties
            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase));
        return string.Join(", ", filteredProps.Select(p => $"{p.Type} {p.Name}"));
    }

    private string GenerateUpdateCommandParameters(AggregateConfiguration aggregate)
    {
        // Always include Id for update, then filter other properties
        var filteredProps = aggregate.Properties
            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase));
        var propParams = string.Join(", ", filteredProps.Select(p => $"{p.Type} {p.Name}"));
        return string.IsNullOrEmpty(propParams) ? "Guid Id" : $"Guid Id, {propParams}";
    }

    private string GenerateDefaultCommandParameters(AggregateConfiguration aggregate)
    {
        // For delete operations, always use Guid Id
        return "Guid Id";
    }

    private string GenerateCommandHandler(TemplateConfiguration config, AggregateConfiguration aggregate, string operation)
    {
        var returnType = operation.ToLowerInvariant() switch
        {
            "create" => "Guid",
            "update" => "",
            "delete" => "",
            _ => ""
        };

        var handlerLogic = operation.ToLowerInvariant() switch
        {
            "create" => GenerateCreateLogic(config, aggregate),
            "update" => GenerateUpdateLogic(aggregate),
            "delete" => GenerateDeleteLogic(aggregate),
            _ => $"// TODO: Implement {operation} logic"
        };

        return $@"using {config.Namespace}.Domain.Entities;
using {config.Namespace}.Application.Common;
using MassTransit;

namespace {config.Namespace}.Application.{aggregate.Name}.Commands.{operation}{aggregate.Name};

public class {operation}{aggregate.Name}CommandHandler : IConsumer<{operation}{aggregate.Name}Command>
{{
    private readonly IRepository<{config.Namespace}.Domain.Entities.{aggregate.Name}> _repository;

    public {operation}{aggregate.Name}CommandHandler(IRepository<{config.Namespace}.Domain.Entities.{aggregate.Name}> repository)
    {{
        _repository = repository;
    }}

    public async Task Consume(ConsumeContext<{operation}{aggregate.Name}Command> context)
    {{
        var command = context.Message;
        {handlerLogic}
        
        {(returnType == "Guid" ? "await context.RespondAsync(entity.Id);" : "")}
    }}
}}";
    }

    private string GenerateCommandValidator(TemplateConfiguration config, AggregateConfiguration aggregate, string operation)
    {
        var validationRules = operation.ToLowerInvariant() switch
        {
            "create" => GenerateCreateValidationRules(aggregate),
            "update" => GenerateUpdateValidationRules(aggregate),
            _ => "// Add validation rules as needed"
        };

        return $@"using FluentValidation;

namespace {config.Namespace}.Application.{aggregate.Name}.Commands.{operation}{aggregate.Name};

public class {operation}{aggregate.Name}CommandValidator : AbstractValidator<{operation}{aggregate.Name}Command>
{{
    public {operation}{aggregate.Name}CommandValidator()
    {{
        {validationRules}
    }}
}}";
    }

    private string GenerateQuery(TemplateConfiguration config, AggregateConfiguration aggregate, string queryName)
    {
        var parameters = queryName switch
        {
            var name when name.Contains("ById") => "Guid Id",
            var name when name.Contains("WithPaging") => "int Page = 1, int PageSize = 10",
            _ => ""
        };

        return $@"using {config.Namespace}.Application.{aggregate.Name}.DTOs;

namespace {config.Namespace}.Application.{aggregate.Name}.Queries.{queryName};

public record {queryName}Query({parameters});";
    }

    private string GenerateQueryHandler(TemplateConfiguration config, AggregateConfiguration aggregate, string queryName)
    {
        var returnType = queryName switch
        {
            var name when name.Contains("ById") => $"{aggregate.Name}Dto?",
            var name when name.Contains("WithPaging") => $"PagedResult<{aggregate.Name}Dto>",
            _ => $"IReadOnlyList<{aggregate.Name}Dto>"
        };

        var handlerLogic = queryName switch
        {
            var name when name.Contains("ById") => GenerateGetByIdLogic(aggregate),
            var name when name.Contains("WithPaging") => GenerateGetWithPagingLogic(aggregate),
            _ => GenerateGetAllLogic(aggregate)
        };

        return $@"using {config.Namespace}.Application.{aggregate.Name}.DTOs;
using {config.Namespace}.Application.Common;
using MassTransit;

namespace {config.Namespace}.Application.{aggregate.Name}.Queries.{queryName};

public class {queryName}QueryHandler : IConsumer<{queryName}Query>
{{
    private readonly IRepository<{config.Namespace}.Domain.Entities.{aggregate.Name}> _repository;

    public {queryName}QueryHandler(IRepository<{config.Namespace}.Domain.Entities.{aggregate.Name}> repository)
    {{
        _repository = repository;
    }}

    public async Task Consume(ConsumeContext<{queryName}Query> context)
    {{
        var query = context.Message;
        {handlerLogic}
        await context.RespondAsync(result);
    }}

    private {aggregate.Name}Dto MapToDto({config.Namespace}.Domain.Entities.{aggregate.Name} entity)
    {{
        return new {aggregate.Name}Dto
        {{
            Id = entity.Id,
            // TODO: Map other properties
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        }};
    }}
}}";
    }

    private string GenerateDTO(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Filter out properties that we'll add manually to avoid duplicates
        var filteredProperties = aggregate.Properties
            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase))
            .Select(p => $"public {p.Type} {p.Name} {{ get; set; }}");

        var properties = string.Join("\n    ", filteredProperties);

        var hasEnums = config.Domain?.Enums?.Any() == true;
        var enumUsing = hasEnums ? $"using {config.Namespace}.Domain.Enums;\n" : "";

        return $@"{enumUsing}namespace {config.Namespace}.Application.{aggregate.Name}.DTOs;

public class {aggregate.Name}Dto
{{
    public Guid Id {{ get; set; }}
    {properties}
    public DateTime CreatedAt {{ get; set; }}
    public DateTime? UpdatedAt {{ get; set; }}
}}";
    }

    private string GenerateCreateDTO(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Filter out audit properties and Id for create DTO
        var filteredProperties = aggregate.Properties
            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase))
            .Select(p => $"public {p.Type} {p.Name} {{ get; set; }}");

        var properties = string.Join("\n    ", filteredProperties);

        var hasEnums = config.Domain?.Enums?.Any() == true;
        var enumUsing = hasEnums ? $"using {config.Namespace}.Domain.Enums;\n" : "";

        return $@"{enumUsing}namespace {config.Namespace}.Application.{aggregate.Name}.DTOs;

public class Create{aggregate.Name}Dto
{{
    {properties}
}}";
    }

    private string GenerateUpdateDTO(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Filter out audit properties and Id for update DTO
        var filteredProperties = aggregate.Properties
            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase))
            .Select(p => $"public {p.Type} {p.Name} {{ get; set; }}");

        var properties = string.Join("\n    ", filteredProperties);

        var hasEnums = config.Domain?.Enums?.Any() == true;
        var enumUsing = hasEnums ? $"using {config.Namespace}.Domain.Enums;\n" : "";

        return $@"{enumUsing}namespace {config.Namespace}.Application.{aggregate.Name}.DTOs;

public class Update{aggregate.Name}Dto
{{
    {properties}
}}";
    }

    private string GenerateServiceCollectionExtensions(TemplateConfiguration config)
    {
        return $@"using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MassTransit;

namespace {config.Namespace}.Application.Extensions;

public static class ServiceCollectionExtensions
{{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {{
        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Add MassTransit
        services.AddMassTransit(x =>
        {{
            // Add consumers from current assembly
            x.AddConsumers(Assembly.GetExecutingAssembly());
            
            // Configure RabbitMQ transport
            x.UsingRabbitMq((context, cfg) =>
            {{
                cfg.Host(""localhost"", ""/"", h =>
                {{
                    h.Username(""guest"");
                    h.Password(""guest"");
                }});
                
                cfg.ConfigureEndpoints(context);
            }});
        }});
        
        return services;
    }}
}}";
    }

    private string GenerateValidationMiddleware(TemplateConfiguration config)
    {
        return $@"using FluentValidation;
using MassTransit;

namespace {config.Namespace}.Application.Middleware;

public class ValidationFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {{
        _validator = validator;
    }}

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {{
        var validationResult = await _validator.ValidateAsync(context.Message);
        
        if (!validationResult.IsValid)
        {{
            throw new ValidationException(validationResult.Errors);
        }}

        await next.Send(context);
    }}

    public void Probe(ProbeContext context)
    {{
        context.CreateFilterScope(""validation"");
    }}
}}";
    }

    private string GenerateLoggingMiddleware(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.Logging;
using MassTransit;

namespace {config.Namespace}.Application.Middleware;

public class LoggingFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{{
    private readonly ILogger<LoggingFilter<T>> _logger;

    public LoggingFilter(ILogger<LoggingFilter<T>> logger)
    {{
        _logger = logger;
    }}

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {{
        var messageType = typeof(T).Name;
        
        _logger.LogInformation(""Processing message {{MessageType}}"", messageType);
        
        try
        {{
            await next.Send(context);
            _logger.LogInformation(""Successfully processed message {{MessageType}}"", messageType);
        }}
        catch (Exception ex)
        {{
            _logger.LogError(ex, ""Error processing message {{MessageType}}"", messageType);
            throw;
        }}
    }}

    public void Probe(ProbeContext context)
    {{
        context.CreateFilterScope(""logging"");
    }}
}}";
    }

    private string GenerateRepositoryInterface(TemplateConfiguration config)
    {
        return $@"using System.Linq.Expressions;

namespace {config.Namespace}.Application.Common;

public interface IRepository<T> where T : class
{{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
    Task<PagedResult<T>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}}

public class PagedResult<T>
{{
    public IReadOnlyList<T> Items {{ get; set; }} = new List<T>();
    public int TotalCount {{ get; set; }}
    public int Page {{ get; set; }}
    public int PageSize {{ get; set; }}
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}}";
    }

    // Helper methods for generating logic
    private string GenerateCreateLogic(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Only use properties that are not audit fields and not Id for entity creation
        var filteredProperties = aggregate.Properties
            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase));
        
        var constructorParams = string.Join(", ", filteredProperties.Select(p => $"command.{p.Name}"));
        
        // Add Guid.NewGuid() as first parameter for entity id
        var fullConstructorParams = string.IsNullOrEmpty(constructorParams) 
            ? "Guid.NewGuid()" 
            : $"Guid.NewGuid(), {constructorParams}";
        
        return $@"var entity = new {config.Namespace}.Domain.Entities.{aggregate.Name}({fullConstructorParams});
        await _repository.AddAsync(entity, context.CancellationToken);";
    }

    private string GenerateUpdateLogic(AggregateConfiguration aggregate)
    {
        return $@"var entity = await _repository.GetByIdAsync(command.Id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException(""{aggregate.Name} not found"");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, context.CancellationToken);";
    }

    private string GenerateDeleteLogic(AggregateConfiguration aggregate)
    {
        return $@"var entity = await _repository.GetByIdAsync(command.id, context.CancellationToken);
        if (entity == null)
            throw new NotFoundException(""{aggregate.Name} not found"");
            
        await _repository.DeleteAsync(entity, context.CancellationToken);";
    }

    private string GenerateGetByIdLogic(AggregateConfiguration aggregate)
    {
        return $@"var entity = await _repository.GetByIdAsync(query.Id, context.CancellationToken);
        var result = entity == null ? null : MapToDto(entity);";
    }

    private string GenerateGetAllLogic(AggregateConfiguration aggregate)
    {
        return $@"var entities = await _repository.GetAllAsync(context.CancellationToken);
        var result = entities.Select(MapToDto).ToList();";
    }

    private string GenerateGetWithPagingLogic(AggregateConfiguration aggregate)
    {
        return $@"var pagedResult = await _repository.GetPagedAsync(query.Page, query.PageSize, context.CancellationToken);
        var result = new PagedResult<{aggregate.Name}Dto>
        {{
            Items = pagedResult.Items.Select(MapToDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        }};";
    }

    private string GenerateCreateValidationRules(AggregateConfiguration aggregate)
    {
        var rules = new List<string>();
        
        // Only validate properties that will be in the command (filtered)
        var filteredProperties = aggregate.Properties
            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase));
        
        foreach (var property in filteredProperties)
        {
            var propertyName = property.Name; // Use PascalCase directly
            
            if (property.Type == "string")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).NotEmpty().WithMessage(\"{propertyName} is required\");");
                rules.Add($"RuleFor(x => x.{propertyName}).MaximumLength(100).WithMessage(\"{propertyName} cannot exceed 100 characters\");");
            }
            else if (property.Type == "int" || property.Type == "decimal")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).GreaterThan(0).WithMessage(\"{propertyName} must be greater than 0\");");
            }
            else if (property.Type == "Guid")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).NotEmpty().WithMessage(\"{propertyName} is required\");");
            }
        }
        
        return string.Join("\n        ", rules);
    }

    private string GenerateUpdateValidationRules(AggregateConfiguration aggregate)
    {
        var rules = new List<string>();
        
        // Always validate Id for update operations
        rules.Add("RuleFor(x => x.Id).NotEmpty().WithMessage(\"Id is required\");");
        
        // Only validate properties that will be in the command (filtered)
        var filteredProperties = aggregate.Properties
            .Where(p => !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                       !p.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase));
        
        foreach (var property in filteredProperties)
        {
            var propertyName = property.Name; // Use PascalCase directly
            
            if (property.Type == "string")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).NotEmpty().WithMessage(\"{propertyName} is required\");");
                rules.Add($"RuleFor(x => x.{propertyName}).MaximumLength(100).WithMessage(\"{propertyName} cannot exceed 100 characters\");");
            }
            else if (property.Type == "int" || property.Type == "decimal")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).GreaterThan(0).WithMessage(\"{propertyName} must be greater than 0\");");
            }
            else if (property.Type == "Guid")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).NotEmpty().WithMessage(\"{propertyName} is required\");");
            }
        }
        
        return string.Join("\n        ", rules);
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToLowerInvariant(input[0]) + input[1..];
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpperInvariant(input[0]) + input[1..];
    }

    private string GenerateNotFoundException(TemplateConfiguration config)
    {
        return $@"using System;

namespace {config.Namespace}.Application.Common;

public class NotFoundException : Exception
{{
    public NotFoundException(string message) : base(message)
    {{
    }}
}}";
    }
} 