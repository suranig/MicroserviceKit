using EventStoreService.Api.Extensions;
using EventStoreService.Api.Filters;
using EventStoreService.Api.Middleware;
using Wolverine;
using Serilog;
using EventStoreService.Application.Extensions;
using EventStoreService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "EventStoreService API", Version = "v1" });
});

// Add application layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add API extensions
builder.Services.AddApiExtensions(builder.Configuration);



// Add Wolverine
builder.Host.UseWolverine();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventStoreService API V1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseResponseCaching();

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationMiddleware>();

app.UseCors();



// Health checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

// Map controllers
app.MapControllers();

// Metrics endpoint for Prometheus
app.MapGet("/metrics", () => "# Metrics endpoint for monitoring");

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }