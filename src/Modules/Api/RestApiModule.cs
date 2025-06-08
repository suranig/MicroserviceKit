using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.Api;

public class RestApiModule : ITemplateModule
{
    public string Name => "RestApi";
    public string Description => "Generates REST API Controllers with CRUD endpoints, validation, and OpenAPI documentation";

    public bool IsEnabled(TemplateConfiguration config)
    {
        var apiStyle = config.Features?.Api?.Style?.ToLowerInvariant();
        return apiStyle == "controllers" || apiStyle == "both" || apiStyle == "auto";
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var outputPath = context.GetApiProjectPath();

        // Create project structure
        await CreateProjectStructureAsync(outputPath, config);

        // Generate controllers for each aggregate
        if (config.Domain?.Aggregates != null)
        {
            foreach (var aggregate in config.Domain.Aggregates)
            {
                await GenerateControllerAsync(outputPath, config, aggregate);
            }
        }

        // Generate common API infrastructure
        await GenerateApiInfrastructureAsync(outputPath, config);
    }

    private async Task CreateProjectStructureAsync(string outputPath, TemplateConfiguration config)
    {
        Directory.CreateDirectory(outputPath);
        Directory.CreateDirectory(Path.Combine(outputPath, "Controllers"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Models"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Filters"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Middleware"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Extensions"));

        // Generate .csproj file
        var csprojContent = GenerateProjectFile(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, $"{config.MicroserviceName}.Api.csproj"), csprojContent);

        // Generate Program.cs
        var programContent = GenerateProgramFile(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "Program.cs"), programContent);

        // Generate appsettings.json
        var appSettingsContent = GenerateAppSettingsFile(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "appsettings.json"), appSettingsContent);
    }

    private async Task GenerateControllerAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var controllerContent = GenerateController(config, aggregate);
        var controllerPath = Path.Combine(outputPath, "Controllers", $"{aggregate.Name}Controller.cs");
        await File.WriteAllTextAsync(controllerPath, controllerContent);

        // Generate request/response models
        await GenerateApiModelsAsync(outputPath, config, aggregate);
    }

    private async Task GenerateApiModelsAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var modelsPath = Path.Combine(outputPath, "Models");

        // Generate request models
        var createRequestContent = GenerateCreateRequest(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(modelsPath, $"Create{aggregate.Name}Request.cs"), 
            createRequestContent);

        var updateRequestContent = GenerateUpdateRequest(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(modelsPath, $"Update{aggregate.Name}Request.cs"), 
            updateRequestContent);

        // Generate response models
        var responseContent = GenerateResponse(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(modelsPath, $"{aggregate.Name}Response.cs"), 
            responseContent);

        // Generate paged response
        var pagedResponseContent = GeneratePagedResponse(config);
        await File.WriteAllTextAsync(
            Path.Combine(modelsPath, "PagedResponse.cs"), 
            pagedResponseContent);
    }

    private async Task GenerateApiInfrastructureAsync(string outputPath, TemplateConfiguration config)
    {
        // Generate global exception filter
        var exceptionFilterContent = GenerateGlobalExceptionFilter(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Filters", "GlobalExceptionFilter.cs"), 
            exceptionFilterContent);

        // Generate validation filter
        var validationFilterContent = GenerateValidationFilter(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Filters", "ValidationFilter.cs"), 
            validationFilterContent);

        // Generate API extensions
        var apiExtensionsContent = GenerateApiExtensions(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Extensions", "ApiExtensions.cs"), 
            apiExtensionsContent);

        // Generate correlation middleware
        var correlationMiddlewareContent = GenerateCorrelationMiddleware(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Middleware", "CorrelationMiddleware.cs"), 
            correlationMiddlewareContent);
    }

    private string GenerateProjectFile(TemplateConfiguration config)
    {
        return $@"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNetCore.OpenApi"" Version=""8.0.16"" />
    <PackageReference Include=""Swashbuckle.AspNetCore"" Version=""6.6.2"" />
    <PackageReference Include=""WolverineFx"" Version=""3.5.0"" />
    <PackageReference Include=""FluentValidation.AspNetCore"" Version=""11.3.0"" />
    <PackageReference Include=""Serilog.AspNetCore"" Version=""8.0.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Authentication.JwtBearer"" Version=""8.0.16"" />
    <PackageReference Include=""Microsoft.AspNetCore.RateLimiting"" Version=""8.0.16"" />
    <PackageReference Include=""Microsoft.AspNetCore.ResponseCompression"" Version=""2.2.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning"" Version=""5.1.0"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer"" Version=""5.1.0"" />
    <PackageReference Include=""Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore"" Version=""8.0.16"" />
    <PackageReference Include=""AspNetCore.HealthChecks.UI.Client"" Version=""8.0.1"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\Application\{config.MicroserviceName}.Application\{config.MicroserviceName}.Application.csproj"" />
    <ProjectReference Include=""..\..\Infrastructure\{config.MicroserviceName}.Infrastructure\{config.MicroserviceName}.Infrastructure.csproj"" />
  </ItemGroup>

</Project>";
    }

    private string GenerateProgramFile(TemplateConfiguration config)
    {
        var authConfig = config.Features?.Api?.Authentication?.ToLowerInvariant() == "jwt" 
            ? GenerateJwtConfiguration() 
            : "";

        return $@"using {config.Namespace}.Application.Extensions;
using {config.Namespace}.Infrastructure.Extensions;
using {config.Namespace}.Api.Extensions;
using {config.Namespace}.Api.Filters;
using {config.Namespace}.Api.Middleware;
using Wolverine;
using Serilog;

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
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add API extensions
builder.Services.AddApiExtensions(builder.Configuration);

{authConfig}

// Add Wolverine
builder.Host.UseWolverine();

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

// Rate limiting
app.UseRateLimiter();

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

// Add rate limiting to controllers
app.MapControllers().RequireRateLimiting(""ApiPolicy"");

// Metrics endpoint for Prometheus
app.MapGet(""/metrics"", () => ""# Metrics endpoint for monitoring"");

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program {{ }}";
    }

    private string GenerateController(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var controllerName = $"{aggregate.Name}Controller";
        var aggregateLower = aggregate.Name.ToLowerInvariant();

        return $@"using Microsoft.AspNetCore.Mvc;
using {config.Namespace}.Application.{aggregate.Name}.Commands.Create{aggregate.Name};
using {config.Namespace}.Application.{aggregate.Name}.Commands.Update{aggregate.Name};
using {config.Namespace}.Application.{aggregate.Name}.Commands.Delete{aggregate.Name};
using {config.Namespace}.Application.{aggregate.Name}.Queries.Get{aggregate.Name}ById;
using {config.Namespace}.Application.{aggregate.Name}.Queries.Get{aggregate.Name}s;
using {config.Namespace}.Application.{aggregate.Name}.Queries.Get{aggregate.Name}sWithPaging;
using {config.Namespace}.Application.{aggregate.Name}.DTOs;
using {config.Namespace}.Api.Models;
using Wolverine;

namespace {config.Namespace}.Api.Controllers;

[ApiController]
[Route(""api/rest/{aggregateLower}"")]
[Produces(""application/json"")]
[Tags(""{aggregate.Name} Management"")]
public class {controllerName} : ControllerBase
{{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<{controllerName}> _logger;

    public {controllerName}(IMessageBus messageBus, ILogger<{controllerName}> logger)
    {{
        _messageBus = messageBus;
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
        _logger.LogInformation(""Getting {aggregateLower}s with page={{Page}}, pageSize={{PageSize}}"", page, pageSize);
        
        var query = new Get{aggregate.Name}sWithPagingQuery(page, pageSize);
        var result = await _messageBus.InvokeAsync<PagedResult<{aggregate.Name}Dto>>(query, cancellationToken);
        
        var response = new PagedResponse<{aggregate.Name}Response>
        {{
            Items = result.Items.Select(MapToResponse).ToList(),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages
        }};
        
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
    public async Task<ActionResult<{aggregate.Name}Response>> Get{aggregate.Name}ById(
        Guid id, 
        CancellationToken cancellationToken = default)
    {{
        _logger.LogInformation(""Getting {aggregateLower} with id={{Id}}"", id);
        
        var query = new Get{aggregate.Name}ByIdQuery(id);
        var result = await _messageBus.InvokeAsync<{aggregate.Name}Dto?>(query, cancellationToken);
        
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
        _logger.LogInformation(""Creating new {aggregateLower}"");
        
        var command = MapToCreateCommand(request);
        var id = await _messageBus.InvokeAsync<Guid>(command, cancellationToken);
        
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
        _logger.LogInformation(""Updating {aggregateLower} with id={{Id}}"", id);
        
        var command = MapToUpdateCommand(id, request);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
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
        _logger.LogInformation(""Deleting {aggregateLower} with id={{Id}}"", id);
        
        var command = new Delete{aggregate.Name}Command(id);
        await _messageBus.InvokeAsync(command, cancellationToken);
        
        _logger.LogInformation(""{aggregate.Name} with id={{Id}} deleted successfully"", id);
        return NoContent();
    }}

    // Mapping methods
    private static {aggregate.Name}Response MapToResponse({aggregate.Name}Dto dto)
    {{
        return new {aggregate.Name}Response
        {{
            Id = dto.Id,
{string.Join(",\n", aggregate.Properties.Select(p => $"            {p.Name} = dto.{p.Name}"))}
        }};
    }}

    private static Create{aggregate.Name}Command MapToCreateCommand(Create{aggregate.Name}Request request)
    {{
        return new Create{aggregate.Name}Command(
{string.Join(",\n", aggregate.Properties.Select(p => $"            request.{p.Name}"))});
    }}

    private static Update{aggregate.Name}Command MapToUpdateCommand(Guid id, Update{aggregate.Name}Request request)
    {{
        return new Update{aggregate.Name}Command(
            id,
{string.Join(",\n", aggregate.Properties.Select(p => $"            request.{p.Name}"))});
    }}
}}";
    }

    private string GenerateCreateRequest(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var properties = string.Join("\n    ", aggregate.Properties.Select(p => 
            $"public {p.Type} {p.Name} {{ get; set; }}"));

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
        var properties = string.Join("\n    ", aggregate.Properties.Select(p => 
            $"public {p.Type} {p.Name} {{ get; set; }}"));

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
        var properties = string.Join("\n    ", aggregate.Properties.Select(p => 
            $"public {p.Type} {p.Name} {{ get; set; }}"));

        return $@"namespace {config.Namespace}.Api.Models;

/// <summary>
/// Response model for {aggregate.Name}
/// </summary>
public class {aggregate.Name}Response
{{
    public Guid Id {{ get; set; }}
{properties}
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

        // Add Health Checks
        services.AddHealthChecks()
            .AddCheck(""self"", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy())
            .AddDbContextCheck<{config.Namespace}.Infrastructure.Persistence.ApplicationDbContext>();

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

        // Add API Versioning
        services.AddApiVersioning(options =>
        {{
            options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = Microsoft.AspNetCore.Mvc.ApiVersionReader.Combine(
                new Microsoft.AspNetCore.Mvc.QueryStringApiVersionReader(""version""),
                new Microsoft.AspNetCore.Mvc.HeaderApiVersionReader(""X-Version""),
                new Microsoft.AspNetCore.Mvc.UrlSegmentApiVersionReader()
            );
        }}).AddVersionedApiExplorer(setup =>
        {{
            setup.GroupNameFormat = ""'v'VVV"";
            setup.SubstituteApiVersionInUrl = true;
        }});

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