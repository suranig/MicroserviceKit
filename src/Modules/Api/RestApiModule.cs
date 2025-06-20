using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.Api;

public class RestApiModule : ITemplateModule
{
    public string Name => "RestApi";
    public string Description => "Generates REST API Controllers with CRUD endpoints, validation, and OpenAPI documentation";

    public bool IsEnabled(TemplateConfiguration config)
    {
        var decisions = ArchitectureRules.MakeDecisions(config);
        // API module is always enabled - it generates either controllers or minimal API based on decisions
        return decisions.ApiStyle == ApiStyle.Controllers || 
               decisions.ApiStyle == ApiStyle.MinimalApi || 
               decisions.ApiStyle == ApiStyle.Both;
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var outputPath = context.GetApiProjectPath();

        // Create project structure
        await CreateProjectStructureAsync(outputPath, config, context);

        // Generate controllers for each aggregate
        if (config.Domain?.Aggregates != null)
        {
            foreach (var aggregate in config.Domain.Aggregates)
            {
                await GenerateControllerAsync(outputPath, config, aggregate, context);
            }
        }

        // Generate common API infrastructure
        await GenerateApiInfrastructureAsync(outputPath, config, context);
    }

    private async Task CreateProjectStructureAsync(string outputPath, TemplateConfiguration config, GenerationContext context)
    {
        Directory.CreateDirectory(outputPath);
        Directory.CreateDirectory(Path.Combine(outputPath, "Controllers"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Models"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Filters"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Middleware"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Extensions"));

        // Generate .csproj file
        var csprojContent = GenerateProjectFile(config);
        await context.WriteFileAsync($"src/Api/{config.MicroserviceName}.Api.csproj", csprojContent);

        // Generate Program.cs
        var programContent = GenerateProgramFile(config);
        await context.WriteFileAsync($"src/Api/Program.cs", programContent);

        // Generate appsettings.json
        var appSettingsContent = GenerateAppSettingsFile(config);
        await context.WriteFileAsync($"src/Api/appsettings.json", appSettingsContent);
    }

    private async Task GenerateControllerAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate, GenerationContext context)
    {
        var controllerContent = GenerateController(config, aggregate);
        await context.WriteFileAsync($"src/Api/Controllers/{aggregate.Name}Controller.cs", controllerContent);

        // Generate request/response models
        await GenerateApiModelsAsync(outputPath, config, aggregate, context);
    }

    private async Task GenerateApiModelsAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate, GenerationContext context)
    {
        var modelsPath = Path.Combine(outputPath, "Models");

        // Generate request models
        var createRequestContent = GenerateCreateRequest(config, aggregate);
        await context.WriteFileAsync(
            $"src/Api/Models/Create{aggregate.Name}Request.cs", 
            createRequestContent);

        var updateRequestContent = GenerateUpdateRequest(config, aggregate);
        await context.WriteFileAsync(
            $"src/Api/Models/Update{aggregate.Name}Request.cs", 
            updateRequestContent);

        // Generate response models
        var responseContent = GenerateResponse(config, aggregate);
        await context.WriteFileAsync(
            $"src/Api/Models/{aggregate.Name}Response.cs", 
            responseContent);

        // Generate paged response
        var pagedResponseContent = GeneratePagedResponse(config);
        await context.WriteFileAsync(
            $"src/Api/Models/PagedResponse.cs", 
            pagedResponseContent);
    }

    private async Task GenerateApiInfrastructureAsync(string outputPath, TemplateConfiguration config, GenerationContext context)
    {
        // Generate global exception filter
        var exceptionFilterContent = GenerateGlobalExceptionFilter(config);
        await context.WriteFileAsync(
            $"src/Api/Filters/GlobalExceptionFilter.cs", 
            exceptionFilterContent);

        // Generate validation filter
        var validationFilterContent = GenerateValidationFilter(config);
        await context.WriteFileAsync(
            $"src/Api/Filters/ValidationFilter.cs", 
            validationFilterContent);

        // Generate API extensions
        var apiExtensionsContent = GenerateApiExtensions(config);
        await context.WriteFileAsync(
            $"src/Api/Extensions/ApiExtensions.cs", 
            apiExtensionsContent);

        // Generate correlation middleware
        var correlationMiddlewareContent = GenerateCorrelationMiddleware(config);
        await context.WriteFileAsync(
            $"src/Api/Middleware/CorrelationMiddleware.cs", 
            correlationMiddlewareContent);
    }

    private string GenerateProjectFile(TemplateConfiguration config)
    {
        var decisions = ArchitectureRules.MakeDecisions(config);
        
        // Generate project references based on enabled modules
        var projectReferences = new List<string>();
        
        if (decisions.EnableDDD)
        {
            projectReferences.Add($@"    <ProjectReference Include=""..\Domain\{config.MicroserviceName}.Domain.csproj"" />");
        }
        
        if (decisions.EnableCQRS && decisions.ArchitectureLevel != ArchitectureLevel.Minimal)
        {
            projectReferences.Add($@"    <ProjectReference Include=""..\Application\{config.MicroserviceName}.Application.csproj"" />");
        }
        
        if (decisions.EnableInfrastructure)
        {
            projectReferences.Add($@"    <ProjectReference Include=""..\Infrastructure\{config.MicroserviceName}.Infrastructure.csproj"" />");
        }

        var projectReferencesSection = projectReferences.Any() 
            ? $@"
  <ItemGroup>
{string.Join("\n", projectReferences)}
  </ItemGroup>"
            : "";

        return $@"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNetCore.OpenApi"" Version=""8.0.0"" />
    <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""6.6.2"" />

    <PackageReference Include=""FluentValidation.AspNetCore"" Version=""11.3.0"" />
    <PackageReference Include=""Serilog.AspNetCore"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.JwtBearer"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.ResponseCompression"" Version=""2.2.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning"" Version=""5.1.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer"" Version=""5.1.0"" />
    <PackageReference Include=""Microsoft.Extensions.Diagnostics.HealthChecks"" Version=""8.0.10"" />
    <PackageReference Include=""AspNetCore.HealthChecks.UI.Client"" Version=""8.0.1"" />
    <PackageReference Include=""MassTransit"" Version=""8.2.0"" />
    <PackageReference Include=""MassTransit.RabbitMQ"" Version=""8.2.0"" />
  </ItemGroup>{projectReferencesSection}

</Project>";
    }

    private string GenerateProgramFile(TemplateConfiguration config)
    {
        var decisions = ArchitectureRules.MakeDecisions(config);
        var authConfig = config.Features?.Api?.Authentication?.ToLowerInvariant() == "jwt" 
            ? GenerateJwtConfiguration() 
            : "";

        // Generate using statements based on enabled modules
        var usingStatements = new List<string>
        {
            $"using {config.Namespace}.Api.Extensions;",
            $"using {config.Namespace}.Api.Filters;",
            $"using {config.Namespace}.Api.Middleware;",
            "using MassTransit;",
            "using Serilog;"
        };

        if (decisions.EnableCQRS && decisions.ArchitectureLevel != ArchitectureLevel.Minimal)
        {
            usingStatements.Add($"using {config.Namespace}.Application.Extensions;");
        }

        if (decisions.EnableInfrastructure)
        {
            usingStatements.Add($"using {config.Namespace}.Infrastructure.Extensions;");
        }

        // Generate service registrations based on enabled modules
        var serviceRegistrations = new List<string>();

        if (decisions.EnableCQRS && decisions.ArchitectureLevel != ArchitectureLevel.Minimal)
        {
            serviceRegistrations.Add("builder.Services.AddApplication();");
        }

        if (decisions.EnableInfrastructure)
        {
            serviceRegistrations.Add("builder.Services.AddInfrastructure(builder.Configuration);");
        }

        var allServiceRegistrations = string.Join("\n", serviceRegistrations);

        return $@"{string.Join("\n", usingStatements)}

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services
builder.Services.AddControllers(options =>
{{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<ValidationFilter>();
}});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{{
    c.SwaggerDoc(""v1"", new() {{ Title = ""{config.MicroserviceName} API"", Version = ""v1"" }});
}});

// Add application layers
{allServiceRegistrations}

// Add API extensions
builder.Services.AddApiExtensions(builder.Configuration);

{authConfig}

// Add MassTransit
builder.Services.AddMassTransit(x =>
{{
    x.UsingRabbitMq((context, cfg) =>
    {{
        cfg.Host(""localhost"", ""/vhost"", h =>
        {{
            h.Username(""guest"");
            h.Password(""guest"");
        }});
        cfg.ConfigureEndpoints(context);
    }});
}});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {{
        c.SwaggerEndpoint(""/swagger/v1/swagger.json"", ""{config.MicroserviceName} API V1"");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    }});
}}

// Security headers
app.Use(async (context, next) =>
{{
    context.Response.Headers.Add(""X-Content-Type-Options"", ""nosniff"");
    context.Response.Headers.Add(""X-Frame-Options"", ""DENY"");
    context.Response.Headers.Add(""X-XSS-Protection"", ""1; mode=block"");
    context.Response.Headers.Add(""Referrer-Policy"", ""strict-origin-when-cross-origin"");
    await next();
}});

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseResponseCaching();

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationMiddleware>();

app.UseCors();

{(config.Features?.Api?.Authentication?.ToLowerInvariant() == "jwt" ? "app.UseAuthentication();\napp.UseAuthorization();" : "")}

// Health checks
app.MapHealthChecks(""/health"");
app.MapHealthChecks(""/health/ready"", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{{
    Predicate = check => check.Tags.Contains(""ready"")
}});
app.MapHealthChecks(""/health/live"", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{{
    Predicate = _ => false
}});

// Map controllers
app.MapControllers();

// Metrics endpoint for Prometheus
app.MapGet(""/metrics"", () => ""# Metrics endpoint for monitoring"");

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program {{ }}";
    }

    private string GenerateController(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var decisions = ArchitectureRules.MakeDecisions(config);
        
        // For minimal architecture, generate simple controller without CQRS
        if (decisions.ArchitectureLevel == ArchitectureLevel.Minimal)
        {
            return GenerateMinimalController(config, aggregate);
        }
        
        // For standard/enterprise architecture, generate CQRS controller
        return GenerateCQRSController(config, aggregate);
    }

    private string GenerateMinimalController(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Generate using statements for minimal controller
        var usingStatements = new List<string>
        {
            "using Microsoft.AspNetCore.Mvc;",
            $"using {config.Namespace}.Domain.Entities;",
            $"using {config.Namespace}.Api.Models;"
        };

        var usings = string.Join("\n", usingStatements);

        return $@"{usings}

namespace {config.Namespace}.Api.Controllers;

[ApiController]
[Route(""api/rest/{aggregate.Name.ToLowerInvariant()}"")]
[Produces(""application/json"")]
[Tags(""{aggregate.Name} Management"")]
public class {aggregate.Name}Controller : ControllerBase
{{
    private readonly ILogger<{aggregate.Name}Controller> _logger;
    private static readonly List<{aggregate.Name}> _data = new(); // In-memory storage for minimal service

    public {aggregate.Name}Controller(ILogger<{aggregate.Name}Controller> logger)
    {{
        _logger = logger;
    }}

    /// <summary>
    /// Get all {aggregate.Name.ToLowerInvariant()}s
    /// </summary>
    /// <returns>List of {aggregate.Name.ToLowerInvariant()}s</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<{aggregate.Name}Response>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<{aggregate.Name}Response>> Get{aggregate.Name}s()
    {{
        _logger.LogInformation(""Getting all {aggregate.Name.ToLowerInvariant()}s"");
        
        var response = _data.Select(MapToResponse).ToList();
        return Ok(response);
    }}

    /// <summary>
    /// Get {aggregate.Name.ToLowerInvariant()} by ID
    /// </summary>
    /// <param name=""id"">{aggregate.Name} ID</param>
    /// <returns>{aggregate.Name} details</returns>
    [HttpGet(""{{id:guid}}"")]
    [ProducesResponseType(typeof({aggregate.Name}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<{aggregate.Name}Response> Get{aggregate.Name}ById(Guid id)
    {{
        _logger.LogInformation(""Getting {aggregate.Name.ToLowerInvariant()} with id={{Id}}"", id);
        
        var entity = _data.FirstOrDefault(x => x.Id == id);
        if (entity == null)
        {{
            _logger.LogWarning(""{aggregate.Name} with id={{Id}} not found"", id);
            return NotFound();
        }}
        
        return Ok(MapToResponse(entity));
    }}

    /// <summary>
    /// Create new {aggregate.Name.ToLowerInvariant()}
    /// </summary>
    /// <param name=""request"">Create {aggregate.Name.ToLowerInvariant()} request</param>
    /// <returns>Created {aggregate.Name.ToLowerInvariant()} ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Guid> Create{aggregate.Name}([FromBody] Create{aggregate.Name}Request request)
    {{
        _logger.LogInformation(""Creating new {aggregate.Name.ToLowerInvariant()}"");
        
        var entity = new {aggregate.Name}(
            Guid.NewGuid(),
            request.Name,
            request.Description);
        
        _data.Add(entity);
        
        _logger.LogInformation(""{aggregate.Name} created with id={{Id}}"", entity.Id);
        return CreatedAtAction(nameof(Get{aggregate.Name}ById), new {{ id = entity.Id }}, entity.Id);
    }}

    /// <summary>
    /// Update existing {aggregate.Name.ToLowerInvariant()}
    /// </summary>
    /// <param name=""id"">{aggregate.Name} ID</param>
    /// <param name=""request"">Update {aggregate.Name.ToLowerInvariant()} request</param>
    /// <returns>No content</returns>
    [HttpPut(""{{id:guid}}"")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Update{aggregate.Name}(Guid id, [FromBody] Update{aggregate.Name}Request request)
    {{
        _logger.LogInformation(""Updating {aggregate.Name.ToLowerInvariant()} with id={{Id}}"", id);
        
        var entity = _data.FirstOrDefault(x => x.Id == id);
        if (entity == null)
        {{
            _logger.LogWarning(""{aggregate.Name} with id={{Id}} not found"", id);
            return NotFound();
        }}

        // Update entity properties (simplified for minimal service)
        // In real implementation, you would call entity.Update() methods
        
        _logger.LogInformation(""{aggregate.Name} with id={{Id}} updated successfully"", id);
        return NoContent();
    }}

    /// <summary>
    /// Delete {aggregate.Name.ToLowerInvariant()}
    /// </summary>
    /// <param name=""id"">{aggregate.Name} ID</param>
    /// <returns>No content</returns>
    [HttpDelete(""{{id:guid}}"")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete{aggregate.Name}(Guid id)
    {{
        _logger.LogInformation(""Deleting {aggregate.Name.ToLowerInvariant()} with id={{Id}}"", id);
        
        var entity = _data.FirstOrDefault(x => x.Id == id);
        if (entity == null)
        {{
            _logger.LogWarning(""{aggregate.Name} with id={{Id}} not found"", id);
            return NotFound();
        }}

        _data.Remove(entity);
        
        _logger.LogInformation(""{aggregate.Name} with id={{Id}} deleted successfully"", id);
        return NoContent();
    }}

    // Mapping methods
    private static {aggregate.Name}Response MapToResponse({aggregate.Name} entity)
    {{
        return new {aggregate.Name}Response
        {{
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        }};
    }}
}}";
    }

    private string GenerateCQRSController(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Generate using statements for CQRS controller
        var usingStatements = new List<string>
        {
            "using Microsoft.AspNetCore.Mvc;",
            $"using {config.Namespace}.Application.{aggregate.Name}.Commands.Create{aggregate.Name};",
            $"using {config.Namespace}.Application.{aggregate.Name}.Commands.Update{aggregate.Name};",
            $"using {config.Namespace}.Application.{aggregate.Name}.Commands.Delete{aggregate.Name};",
            $"using {config.Namespace}.Application.{aggregate.Name}.Queries.Get{aggregate.Name}ById;",
            $"using {config.Namespace}.Application.{aggregate.Name}.Queries.Get{aggregate.Name}s;",
            $"using {config.Namespace}.Application.{aggregate.Name}.Queries.Get{aggregate.Name}sWithPaging;",
            $"using {config.Namespace}.Application.{aggregate.Name}.DTOs;",
            $"using {config.Namespace}.Api.Models;",
            "using MassTransit;"
        };

        var usings = string.Join("\n", usingStatements);

        return $@"{usings}

namespace {config.Namespace}.Api.Controllers;

[ApiController]
[Route(""api/rest/{aggregate.Name.ToLowerInvariant()}"")]
[Produces(""application/json"")]
[Tags(""{aggregate.Name} Management"")]
public class {aggregate.Name}Controller : ControllerBase
{{
    private readonly IBus _bus;
    private readonly ILogger<{aggregate.Name}Controller> _logger;

    public {aggregate.Name}Controller(IBus bus, ILogger<{aggregate.Name}Controller> logger)
    {{
        _bus = bus;
        _logger = logger;
    }}

    /// <summary>
    /// Get all {aggregate.Name.ToLowerInvariant()}s with paging
    /// </summary>
    /// <param name=""page"">Page number (default: 1)</param>
    /// <param name=""pageSize"">Page size (default: 10)</param>
    /// <returns>Paged list of {aggregate.Name.ToLowerInvariant()}s</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<{aggregate.Name}Response>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<{aggregate.Name}Response>>> Get{aggregate.Name}s(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Getting {aggregate.Name.ToLowerInvariant()}s with page={{Page}}, pageSize={{PageSize}}"", page, pageSize);
        
        var query = new Get{aggregate.Name}sWithPagingQuery(page, pageSize);
        var client = _bus.CreateRequestClient<Get{aggregate.Name}sWithPagingQuery>();
        var response = await client.GetResponse<PagedResult<{aggregate.Name}Dto>>(query, cancellationToken);
        var result = response.Message;
        
        var pagedResponse = new PagedResponse<{aggregate.Name}Response>
        {{
            Items = result.Items.Select(MapToResponse).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        }};
        
        return Ok(pagedResponse);
    }}

    /// <summary>
    /// Get {aggregate.Name.ToLowerInvariant()} by ID
    /// </summary>
    /// <param name=""id"">{aggregate.Name} ID</param>
    /// <returns>{aggregate.Name} details</returns>
    [HttpGet(""{{id:guid}}"")]
    [ProducesResponseType(typeof({aggregate.Name}Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<{aggregate.Name}Response>> Get{aggregate.Name}ById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Getting {aggregate.Name.ToLowerInvariant()} with id={{Id}}"", id);
        
        var query = new Get{aggregate.Name}ByIdQuery(id);
        var client = _bus.CreateRequestClient<Get{aggregate.Name}ByIdQuery>();
        var response = await client.GetResponse<{aggregate.Name}Dto?>(query, cancellationToken);
        var result = response.Message;
        
        if (result == null)
        {{
            _logger.LogWarning(""{aggregate.Name} with id={{Id}} not found"", id);
            return NotFound();
        }}
        
        return Ok(MapToResponse(result));
    }}

    /// <summary>
    /// Create new {aggregate.Name.ToLowerInvariant()}
    /// </summary>
    /// <param name=""request"">Create {aggregate.Name.ToLowerInvariant()} request</param>
    /// <returns>Created {aggregate.Name.ToLowerInvariant()} ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create{aggregate.Name}(
        [FromBody] Create{aggregate.Name}Request request,
        CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Creating new {aggregate.Name.ToLowerInvariant()}"");
        
        var command = MapToCreateCommand(request);
        var client = _bus.CreateRequestClient<Create{aggregate.Name}Command>();
        var response = await client.GetResponse<Guid>(command, cancellationToken);
        var id = response.Message;
        
        _logger.LogInformation(""{aggregate.Name} created with id={{Id}}"", id);
        return CreatedAtAction(nameof(Get{aggregate.Name}ById), new {{ id }}, id);
    }}

    /// <summary>
    /// Update existing {aggregate.Name.ToLowerInvariant()}
    /// </summary>
    /// <param name=""id"">{aggregate.Name} ID</param>
    /// <param name=""request"">Update {aggregate.Name.ToLowerInvariant()} request</param>
    /// <returns>No content</returns>
    [HttpPut(""{{id:guid}}"")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update{aggregate.Name}(
        Guid id,
        [FromBody] Update{aggregate.Name}Request request,
        CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Updating {aggregate.Name.ToLowerInvariant()} with id={{Id}}"", id);
        
        var command = MapToUpdateCommand(id, request);
        var client = _bus.CreateRequestClient<Update{aggregate.Name}Command>();
        await client.GetResponse<MediatR.Unit>(command, cancellationToken);
        
        _logger.LogInformation(""{aggregate.Name} with id={{Id}} updated successfully"", id);
        return NoContent();
    }}

    /// <summary>
    /// Delete {aggregate.Name.ToLowerInvariant()}
    /// </summary>
    /// <param name=""id"">{aggregate.Name} ID</param>
    /// <returns>No content</returns>
    [HttpDelete(""{{id:guid}}"")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete{aggregate.Name}(
        Guid id,
        CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Deleting {aggregate.Name.ToLowerInvariant()} with id={{Id}}"", id);
        
        var command = new Delete{aggregate.Name}Command(id);
        var client = _bus.CreateRequestClient<Delete{aggregate.Name}Command>();
        await client.GetResponse<MediatR.Unit>(command, cancellationToken);
        
        _logger.LogInformation(""{aggregate.Name} with id={{Id}} deleted successfully"", id);
        return NoContent();
    }}

    // Mapping methods
    private static {aggregate.Name}Response MapToResponse({aggregate.Name}Dto dto)
    {{
        return new {aggregate.Name}Response
        {{
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description
        }};
    }}

    private static Create{aggregate.Name}Command MapToCreateCommand(Create{aggregate.Name}Request request)
    {{
        return new Create{aggregate.Name}Command(
            request.Name,
            request.Description);
    }}

    private static Update{aggregate.Name}Command MapToUpdateCommand(Guid id, Update{aggregate.Name}Request request)
    {{
        return new Update{aggregate.Name}Command(
            id,
            request.Name,
            request.Description);
    }}
}}";
    }

    private string GenerateCreateRequest(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Filter out Id, CreatedAt, UpdatedAt from create request as they are auto-generated
        var filteredProperties = aggregate.Properties
            .Where(p => p.Name != "Id" && p.Name != "CreatedAt" && p.Name != "UpdatedAt")
            .Select(p => $"public {p.Type} {p.Name} {{ get; set; }}")
            .ToList();

        var properties = filteredProperties.Any() 
            ? string.Join("\n    ", filteredProperties)
            : "// No additional properties";

        return $@"using System.ComponentModel.DataAnnotations;

namespace {config.Namespace}.Api.Models;

/// <summary>
/// Request model for creating {aggregate.Name}
/// </summary>
public class Create{aggregate.Name}Request
{{
    {properties}
}}";
    }

    private string GenerateUpdateRequest(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Filter out Id, CreatedAt, UpdatedAt from update request
        var filteredProperties = aggregate.Properties
            .Where(p => p.Name != "Id" && p.Name != "CreatedAt" && p.Name != "UpdatedAt")
            .Select(p => $"public {p.Type} {p.Name} {{ get; set; }}")
            .ToList();

        var properties = filteredProperties.Any() 
            ? string.Join("\n    ", filteredProperties)
            : "// No additional properties";

        return $@"using System.ComponentModel.DataAnnotations;

namespace {config.Namespace}.Api.Models;

/// <summary>
/// Request model for updating {aggregate.Name}
/// </summary>
public class Update{aggregate.Name}Request
{{
    {properties}
}}";
    }

    private string GenerateResponse(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Filter out Id, CreatedAt, UpdatedAt from aggregate properties as they are added separately
        var filteredProperties = aggregate.Properties
            .Where(p => p.Name != "Id" && p.Name != "CreatedAt" && p.Name != "UpdatedAt")
            .Select(p => $"public {p.Type} {p.Name} {{ get; set; }}")
            .ToList();

        var properties = filteredProperties.Any() 
            ? "\n    " + string.Join("\n    ", filteredProperties)
            : "";

        return $@"namespace {config.Namespace}.Api.Models;

/// <summary>
/// Response model for {aggregate.Name}
/// </summary>
public class {aggregate.Name}Response
{{
    public Guid Id {{ get; set; }}{properties}
    public DateTime CreatedAt {{ get; set; }}
    public DateTime? UpdatedAt {{ get; set; }}
}}";
    }

    private string GeneratePagedResponse(TemplateConfiguration config)
    {
        return $@"namespace {config.Namespace}.Api.Models;

/// <summary>
/// Generic paged response model
/// </summary>
/// <typeparam name=""T"">Type of items</typeparam>
public class PagedResponse<T>
{{
    public IReadOnlyList<T> Items {{ get; set; }} = new List<T>();
    public int TotalCount {{ get; set; }}
    public int Page {{ get; set; }}
    public int PageSize {{ get; set; }}
    public int TotalPages {{ get; set; }}
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}}";
    }

    private string GenerateGlobalExceptionFilter(TemplateConfiguration config)
    {
        return $@"using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;

namespace {config.Namespace}.Api.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {{
        _logger = logger;
    }}

    public void OnException(ExceptionContext context)
    {{
        var exception = context.Exception;
        
        _logger.LogError(exception, ""An unhandled exception occurred"");

        var result = exception switch
        {{
            ValidationException validationEx => HandleValidationException(validationEx),
            ArgumentException argumentEx => HandleArgumentException(argumentEx),
            InvalidOperationException invalidOpEx => HandleInvalidOperationException(invalidOpEx),
            _ => HandleGenericException(exception)
        }};

        context.Result = result;
        context.ExceptionHandled = true;
    }}

    private static IActionResult HandleValidationException(ValidationException exception)
    {{
        var errors = exception.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new
        {{
            type = ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
            title = ""One or more validation errors occurred."",
            status = 400,
            errors
        }});
    }}

    private static IActionResult HandleArgumentException(ArgumentException exception)
    {{
        return new BadRequestObjectResult(new
        {{
            type = ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
            title = ""Bad Request"",
            status = 400,
            detail = exception.Message
        }});
    }}

    private static IActionResult HandleInvalidOperationException(InvalidOperationException exception)
    {{
        return new ConflictObjectResult(new
        {{
            type = ""https://tools.ietf.org/html/rfc7231#section-6.5.8"",
            title = ""Conflict"",
            status = 409,
            detail = exception.Message
        }});
    }}

    private static IActionResult HandleGenericException(Exception exception)
    {{
        return new ObjectResult(new
        {{
            type = ""https://tools.ietf.org/html/rfc7231#section-6.6.1"",
            title = ""An error occurred while processing your request."",
            status = 500
        }})
        {{
            StatusCode = 500
        }};
    }}
}}";
    }

    private string GenerateValidationFilter(TemplateConfiguration config)
    {
        return $@"using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace {config.Namespace}.Api.Filters;

public class ValidationFilter : IActionFilter
{{
    public void OnActionExecuting(ActionExecutingContext context)
    {{
        if (!context.ModelState.IsValid)
        {{
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            var result = new BadRequestObjectResult(new
            {{
                type = ""https://tools.ietf.org/html/rfc7231#section-6.5.1"",
                title = ""One or more validation errors occurred."",
                status = 400,
                errors
            }});

            context.Result = result;
        }}
    }}

    public void OnActionExecuted(ActionExecutedContext context)
    {{
        // No implementation needed
    }}
}}";
    }

    private string GenerateApiExtensions(TemplateConfiguration config)
    {
        var decisions = ArchitectureRules.MakeDecisions(config);
        
        // Generate health checks section based on enabled modules
        var healthChecks = decisions.EnableInfrastructure 
            ? $@"        // Add Health Checks
        services.AddHealthChecks()
            .AddCheck(""self"", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
            .AddDbContextCheck<{config.Namespace}.Infrastructure.Persistence.ApplicationDbContext>();"
            : $@"        // Add Health Checks
        services.AddHealthChecks()
            .AddCheck(""self"", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());";

        // Generate API versioning section based on architecture level
        var apiVersioning = decisions.ArchitectureLevel != ArchitectureLevel.Minimal
            ? $@"        // Add API Versioning
        services.AddApiVersioning(options =>
        {{
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = Microsoft.AspNetCore.Mvc.ApiVersionReader.Combine(
                new Microsoft.AspNetCore.Mvc.QueryStringApiVersionReader(""version""),
                new Microsoft.AspNetCore.Mvc.HeaderApiVersionReader(""X-Version"")
            );
        }});"
            : "        // API Versioning not included for minimal architecture";

        return $@"using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace {config.Namespace}.Api.Extensions;

public static class ApiExtensions
{{
    public static IServiceCollection AddApiExtensions(this IServiceCollection services, IConfiguration configuration)
    {{
        // Add CORS
        services.AddCors(options =>
        {{
            options.AddDefaultPolicy(builder =>
            {{
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }});
        }});

        // Add Rate Limiting
        services.AddRateLimiter(options =>
        {{
            options.AddFixedWindowLimiter(""ApiPolicy"", limiterOptions =>
            {{
                limiterOptions.PermitLimit = configuration.GetValue<int>(""RateLimiting:PermitLimit"", 100);
                limiterOptions.Window = TimeSpan.FromMinutes(configuration.GetValue<int>(""RateLimiting:WindowMinutes"", 1));
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = configuration.GetValue<int>(""RateLimiting:QueueLimit"", 10);
            }});

            options.AddConcurrencyLimiter(""ConcurrencyPolicy"", limiterOptions =>
            {{
                limiterOptions.PermitLimit = configuration.GetValue<int>(""RateLimiting:ConcurrencyLimit"", 50);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = configuration.GetValue<int>(""RateLimiting:QueueLimit"", 10);
            }});

            options.OnRejected = async (context, token) =>
            {{
                context.HttpContext.Response.StatusCode = 429;
                await context.HttpContext.Response.WriteAsync(""Too many requests. Please try again later."", token);
            }};
        }});

        // Add Response Compression
        services.AddResponseCompression(options =>
        {{
            options.EnableForHttps = true;
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
        }});

        // Add Response Caching
        services.AddResponseCaching();

        // Add Memory Cache
        services.AddMemoryCache();

{healthChecks}

        // Configure Swagger
        services.AddSwaggerGen(c =>
        {{
            c.SwaggerDoc(""v1"", new OpenApiInfo
            {{
                Title = ""{config.MicroserviceName} API"",
                Version = ""v1"",
                Description = ""API for {config.MicroserviceName} microservice"",
                Contact = new OpenApiContact
                {{
                    Name = ""API Support"",
                    Email = ""support@example.com""
                }}
            }});

            // Add security definition for JWT
            c.AddSecurityDefinition(""Bearer"", new OpenApiSecurityScheme
            {{
                Description = ""JWT Authorization header using the Bearer scheme"",
                Name = ""Authorization"",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = ""Bearer""
            }});

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {{
                {{
                    new OpenApiSecurityScheme
                    {{
                        Reference = new OpenApiReference
                        {{
                            Type = ReferenceType.SecurityScheme,
                            Id = ""Bearer""
                        }}
                    }},
                    Array.Empty<string>()
                }}
            }});

            // Add XML comments if available
            var xmlFile = $""{{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}}.xml"";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {{
                c.IncludeXmlComments(xmlPath);
            }}
        }});

{apiVersioning}

        return services;
    }}
}}";
    }

    private string GenerateCorrelationMiddleware(TemplateConfiguration config)
    {
        return $@"namespace {config.Namespace}.Api.Middleware;

public class CorrelationMiddleware
{{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;
    private const string CorrelationIdHeader = ""X-Correlation-ID"";

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {{
        _next = next;
        _logger = logger;
    }}

    public async Task InvokeAsync(HttpContext context)
    {{
        var correlationId = GetOrCreateCorrelationId(context);
        
        using (_logger.BeginScope(new Dictionary<string, object> {{ [""CorrelationId""] = correlationId }}))
        {{
            context.Response.Headers.Add(CorrelationIdHeader, correlationId);
            await _next(context);
        }}
    }}

    private static string GetOrCreateCorrelationId(HttpContext context)
    {{
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {{
            return correlationId.FirstOrDefault() ?? Guid.NewGuid().ToString();
        }}

        return Guid.NewGuid().ToString();
    }}
}}";
    }

    private string GenerateAppSettingsFile(TemplateConfiguration config)
    {
        return $@"{{
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }}
  }},
  ""AllowedHosts"": ""*"",
  ""ConnectionStrings"": {{
    ""DefaultConnection"": ""Server=(localdb)\\mssqllocaldb;Database={config.MicroserviceName}Db;Trusted_Connection=true;MultipleActiveResultSets=true""
  }},
  ""RateLimiting"": {{
    ""PermitLimit"": 100,
    ""WindowMinutes"": 1,
    ""QueueLimit"": 10,
    ""ConcurrencyLimit"": 50
  }},
  ""Features"": {{
    ""RateLimiting"": {{
      ""Enabled"": true
    }},
    ""ResponseCompression"": {{
      ""Enabled"": true
    }},
    ""ResponseCaching"": {{
      ""Enabled"": true,
      ""DefaultDurationMinutes"": 5
    }}
  }},
  ""Serilog"": {{
    ""Using"": [""Serilog.Sinks.Console"", ""Serilog.Sinks.File""],
    ""MinimumLevel"": {{
      ""Default"": ""Information"",
      ""Override"": {{
        ""Microsoft"": ""Warning"",
        ""System"": ""Warning""
      }}
    }},
    ""WriteTo"": [
      {{
        ""Name"": ""Console"",
        ""Args"": {{
          ""outputTemplate"": ""[{{Timestamp:HH:mm:ss}} {{Level:u3}}] {{CorrelationId}} {{Message:lj}}{{NewLine}}{{Exception}}""
        }}
      }},
      {{
        ""Name"": ""File"",
        ""Args"": {{
          ""path"": ""logs/{config.MicroserviceName.ToLowerInvariant()}-.log"",
          ""rollingInterval"": ""Day"",
          ""outputTemplate"": ""[{{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}} [{{Level:u3}}] {{CorrelationId}} {{Message:lj}}{{NewLine}}{{Exception}}""
        }}
      }}
    ]
  }}
}}";
    }

    private string GenerateJwtConfiguration()
    {
        return @"
// Add JWT Authentication
builder.Services.AddAuthentication(""Bearer"")
    .AddJwtBearer(""Bearer"", options =>
    {
        options.Authority = builder.Configuration[""Authentication:Authority""];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();";
    }
} 