using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;
using System.Text;

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
        await CreateProjectStructureAsync(outputPath, config);

        // Generate handlers for each aggregate
        if (config.Domain?.Aggregates != null)
        {
            foreach (var aggregate in config.Domain.Aggregates)
            {
                await GenerateAggregateHandlersAsync(outputPath, config, aggregate);
            }
        }

        // Generate common application services
        await GenerateApplicationServicesAsync(outputPath, config);
    }

    private async Task CreateProjectStructureAsync(string outputPath, TemplateConfiguration config)
    {
        Directory.CreateDirectory(outputPath);
        Directory.CreateDirectory(Path.Combine(outputPath, "Common"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Behaviors"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Extensions"));

        // Generate .csproj file
        var csprojContent = GenerateProjectFile(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, $"{config.MicroserviceName}.Application.csproj"), csprojContent);
    }

    private async Task GenerateAggregateHandlersAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var aggregatePath = Path.Combine(outputPath, aggregate.Name);
        Directory.CreateDirectory(aggregatePath);
        Directory.CreateDirectory(Path.Combine(aggregatePath, "Commands"));
        Directory.CreateDirectory(Path.Combine(aggregatePath, "Queries"));
        Directory.CreateDirectory(Path.Combine(aggregatePath, "DTOs"));

        // Generate Commands
        await GenerateCommandsAsync(aggregatePath, config, aggregate);

        // Generate Queries  
        await GenerateQueriesAsync(aggregatePath, config, aggregate);

        // Generate DTOs
        await GenerateDTOsAsync(aggregatePath, config, aggregate);
    }

    private async Task GenerateCommandsAsync(string aggregatePath, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var commandsPath = Path.Combine(aggregatePath, "Commands");

        // Generate CRUD commands
        var crudOperations = new[] { "Create", "Update", "Delete" };
        var customOperations = aggregate.Operations ?? new List<string>();
        var allOperations = crudOperations.Concat(customOperations).Distinct();

        foreach (var operation in allOperations)
        {
            var operationPath = Path.Combine(commandsPath, $"{operation}{aggregate.Name}");
            Directory.CreateDirectory(operationPath);

            // Generate Command
            var commandContent = GenerateCommand(config, aggregate, operation);
            await File.WriteAllTextAsync(
                Path.Combine(operationPath, $"{operation}{aggregate.Name}Command.cs"), 
                commandContent);

            // Generate Handler
            var handlerContent = GenerateCommandHandler(config, aggregate, operation);
            await File.WriteAllTextAsync(
                Path.Combine(operationPath, $"{operation}{aggregate.Name}CommandHandler.cs"), 
                handlerContent);

            // Generate Validator
            var validatorContent = GenerateCommandValidator(config, aggregate, operation);
            await File.WriteAllTextAsync(
                Path.Combine(operationPath, $"{operation}{aggregate.Name}CommandValidator.cs"), 
                validatorContent);
        }
    }

    private async Task GenerateQueriesAsync(string aggregatePath, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var queriesPath = Path.Combine(aggregatePath, "Queries");

        // Generate standard queries
        var queries = new[]
        {
            $"Get{aggregate.Name}ById",
            $"Get{aggregate.Name}s",
            $"Get{aggregate.Name}sWithPaging"
        };

        foreach (var queryName in queries)
        {
            var queryPath = Path.Combine(queriesPath, queryName);
            Directory.CreateDirectory(queryPath);

            // Generate Query
            var queryContent = GenerateQuery(config, aggregate, queryName);
            await File.WriteAllTextAsync(
                Path.Combine(queryPath, $"{queryName}Query.cs"), 
                queryContent);

            // Generate Handler
            var handlerContent = GenerateQueryHandler(config, aggregate, queryName);
            await File.WriteAllTextAsync(
                Path.Combine(queryPath, $"{queryName}QueryHandler.cs"), 
                handlerContent);
        }
    }

    private async Task GenerateDTOsAsync(string aggregatePath, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var dtosPath = Path.Combine(aggregatePath, "DTOs");

        // Generate DTOs
        var dtoContent = GenerateDTO(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(dtosPath, $"{aggregate.Name}Dto.cs"), 
            dtoContent);

        var createDtoContent = GenerateCreateDTO(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(dtosPath, $"Create{aggregate.Name}Dto.cs"), 
            createDtoContent);

        var updateDtoContent = GenerateUpdateDTO(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(dtosPath, $"Update{aggregate.Name}Dto.cs"), 
            updateDtoContent);
    }

    private async Task GenerateApplicationServicesAsync(string outputPath, TemplateConfiguration config)
    {
        // Generate Extensions
        var extensionsContent = GenerateServiceCollectionExtensions(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Extensions", "ServiceCollectionExtensions.cs"), 
            extensionsContent);

        // Generate Behaviors
        var validationBehaviorContent = GenerateValidationBehavior(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Behaviors", "ValidationBehavior.cs"), 
            validationBehaviorContent);

        var loggingBehaviorContent = GenerateLoggingBehavior(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Behaviors", "LoggingBehavior.cs"), 
            loggingBehaviorContent);

        // Generate Common interfaces
        var repositoryInterfaceContent = GenerateRepositoryInterface(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Common", "IRepository.cs"), 
            repositoryInterfaceContent);
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
    <PackageReference Include=""WolverineFx"" Version=""3.5.0"" />
    <PackageReference Include=""FluentValidation"" Version=""11.9.0"" />
    <PackageReference Include=""FluentValidation.DependencyInjectionExtensions"" Version=""11.9.0"" />
    <PackageReference Include=""Microsoft.Extensions.DependencyInjection.Abstractions"" Version=""8.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\Domain\{config.MicroserviceName}.Domain\{config.MicroserviceName}.Domain.csproj"" />
  </ItemGroup>

</Project>";
    }

    private string GenerateCommand(TemplateConfiguration config, AggregateConfiguration aggregate, string operation)
    {
        var parameters = operation.ToLowerInvariant() switch
        {
            "create" => string.Join(", ", aggregate.Properties.Select(p => $"{p.Type} {ToCamelCase(p.Name)}")),
            "update" => $"Guid id, " + string.Join(", ", aggregate.Properties.Select(p => $"{p.Type} {ToCamelCase(p.Name)}")),
            "delete" => "Guid id",
            _ => string.Join(", ", aggregate.Properties.Take(2).Select(p => $"{p.Type} {ToCamelCase(p.Name)}"))
        };

        return $@"namespace {config.Namespace}.Application.{aggregate.Name}.Commands.{operation}{aggregate.Name};

public record {operation}{aggregate.Name}Command({parameters});";
    }

    private string GenerateCommandHandler(TemplateConfiguration config, AggregateConfiguration aggregate, string operation)
    {
        var returnType = operation.ToLowerInvariant() switch
        {
            "create" => "Guid",
            "update" => "void",
            "delete" => "void",
            _ => "void"
        };

        var handlerLogic = operation.ToLowerInvariant() switch
        {
            "create" => GenerateCreateLogic(aggregate),
            "update" => GenerateUpdateLogic(aggregate),
            "delete" => GenerateDeleteLogic(aggregate),
            _ => $"// TODO: Implement {operation} logic"
        };

        return $@"using {config.Namespace}.Domain.Entities;
using {config.Namespace}.Application.Common;

namespace {config.Namespace}.Application.{aggregate.Name}.Commands.{operation}{aggregate.Name};

public class {operation}{aggregate.Name}CommandHandler
{{
    private readonly IRepository<{aggregate.Name}> _repository;

    public {operation}{aggregate.Name}CommandHandler(IRepository<{aggregate.Name}> repository)
    {{
        _repository = repository;
    }}

    public async Task<{returnType}> Handle({operation}{aggregate.Name}Command command, CancellationToken cancellationToken)
    {{
        {handlerLogic}
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
            var name when name.Contains("ById") => "Guid id",
            var name when name.Contains("WithPaging") => "int page = 1, int pageSize = 10",
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

namespace {config.Namespace}.Application.{aggregate.Name}.Queries.{queryName};

public class {queryName}QueryHandler
{{
    private readonly IRepository<{config.Namespace}.Domain.Entities.{aggregate.Name}> _repository;

    public {queryName}QueryHandler(IRepository<{config.Namespace}.Domain.Entities.{aggregate.Name}> repository)
    {{
        _repository = repository;
    }}

    public async Task<{returnType}> Handle({queryName}Query query, CancellationToken cancellationToken)
    {{
        {handlerLogic}
    }}
}}";
    }

    private string GenerateDTO(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var properties = string.Join("\n    ", aggregate.Properties.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));

        return $@"namespace {config.Namespace}.Application.{aggregate.Name}.DTOs;

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
        var properties = string.Join("\n    ", aggregate.Properties.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));

        return $@"namespace {config.Namespace}.Application.{aggregate.Name}.DTOs;

public class Create{aggregate.Name}Dto
{{
    {properties}
}}";
    }

    private string GenerateUpdateDTO(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var properties = string.Join("\n    ", aggregate.Properties.Select(p => $"public {p.Type} {p.Name} {{ get; set; }}"));

        return $@"namespace {config.Namespace}.Application.{aggregate.Name}.DTOs;

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
using Wolverine;

namespace {config.Namespace}.Application.Extensions;

public static class ServiceCollectionExtensions
{{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {{
        // Add FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Add Wolverine behaviors
        services.AddScoped(typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(LoggingBehavior<,>));
        
        return services;
    }}
}}";
    }

    private string GenerateValidationBehavior(TemplateConfiguration config)
    {
        return $@"using FluentValidation;

namespace {config.Namespace}.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
{{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {{
        _validators = validators;
    }}

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {{
        if (_validators.Any())
        {{
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }}

        return await next();
    }}
}}";
    }

    private string GenerateLoggingBehavior(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.Logging;

namespace {config.Namespace}.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>
{{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {{
        _logger = logger;
    }}

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {{
        var requestName = typeof(TRequest).Name;
        
        _logger.LogInformation(""Handling {{RequestName}}"", requestName);
        
        var response = await next();
        
        _logger.LogInformation(""Handled {{RequestName}}"", requestName);
        
        return response;
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
    private string GenerateCreateLogic(AggregateConfiguration aggregate)
    {
        var constructorParams = string.Join(", ", aggregate.Properties.Select(p => $"command.{ToPascalCase(p.Name)}"));
        
        return $@"var entity = new {aggregate.Name}({constructorParams});
        await _repository.AddAsync(entity, cancellationToken);
        return entity.Id;";
    }

    private string GenerateUpdateLogic(AggregateConfiguration aggregate)
    {
        return $@"var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException(""{aggregate.Name} not found"");
            
        // TODO: Update entity properties
        await _repository.UpdateAsync(entity, cancellationToken);";
    }

    private string GenerateDeleteLogic(AggregateConfiguration aggregate)
    {
        return $@"var entity = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (entity == null)
            throw new NotFoundException(""{aggregate.Name} not found"");
            
        await _repository.DeleteAsync(entity, cancellationToken);";
    }

    private string GenerateGetByIdLogic(AggregateConfiguration aggregate)
    {
        return $@"var entity = await _repository.GetByIdAsync(query.Id, cancellationToken);
        return entity == null ? null : MapToDto(entity);";
    }

    private string GenerateGetAllLogic(AggregateConfiguration aggregate)
    {
        return $@"var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();";
    }

    private string GenerateGetWithPagingLogic(AggregateConfiguration aggregate)
    {
        return $@"var result = await _repository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);
        return new PagedResult<{aggregate.Name}Dto>
        {{
            Items = result.Items.Select(MapToDto).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        }};";
    }

    private string GenerateCreateValidationRules(AggregateConfiguration aggregate)
    {
        var rules = new List<string>();
        
        foreach (var property in aggregate.Properties)
        {
            var propertyName = ToPascalCase(property.Name);
            
            if (property.Type == "string")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).NotEmpty().MaximumLength(255);");
            }
            else if (property.Type == "Guid")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).NotEmpty();");
            }
            else if (property.Type == "decimal")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).GreaterThan(0);");
            }
        }
        
        return string.Join("\n        ", rules);
    }

    private string GenerateUpdateValidationRules(AggregateConfiguration aggregate)
    {
        var rules = new List<string> { "RuleFor(x => x.Id).NotEmpty();" };
        
        foreach (var property in aggregate.Properties)
        {
            var propertyName = ToPascalCase(property.Name);
            
            if (property.Type == "string")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).NotEmpty().MaximumLength(255);");
            }
            else if (property.Type == "decimal")
            {
                rules.Add($"RuleFor(x => x.{propertyName}).GreaterThan(0);");
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
} 