using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.Infrastructure;

public class InfrastructureModule : ITemplateModule
{
    public string Name => "Infrastructure";
    public string Description => "Generates Infrastructure layer with Entity Framework DbContext, repositories, and database configuration";

    public bool IsEnabled(TemplateConfiguration config)
    {
        // Infrastructure layer potrzebny dla wszystkich poziomów oprócz minimal
        var level = config.Architecture?.Level?.ToLowerInvariant();
        return level != "minimal";
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;

        // Create project structure
        await CreateProjectStructureAsync(context, config);

        // Generate DbContext
        await GenerateDbContextAsync(context, config);

        // Generate repositories for each aggregate
        if (config.Domain?.Aggregates != null)
        {
            foreach (var aggregate in config.Domain.Aggregates)
            {
                await GenerateRepositoryAsync(context, config, aggregate);
                await GenerateEntityConfigurationAsync(context, config, aggregate);
            }
        }

        // Generate infrastructure extensions
        await GenerateInfrastructureExtensionsAsync(context, config);

        // Generate database configuration
        await GenerateDatabaseConfigurationAsync(context, config);
    }

    private async Task CreateProjectStructureAsync(GenerationContext context, TemplateConfiguration config)
    {
        Directory.CreateDirectory(context.GetInfrastructureProjectPath());
        Directory.CreateDirectory(Path.Combine(context.GetInfrastructureProjectPath(), "Persistence"));
        Directory.CreateDirectory(Path.Combine(context.GetInfrastructureProjectPath(), "Persistence", "Configurations"));
        Directory.CreateDirectory(Path.Combine(context.GetInfrastructureProjectPath(), "Repositories"));
        Directory.CreateDirectory(Path.Combine(context.GetInfrastructureProjectPath(), "Extensions"));
        Directory.CreateDirectory(Path.Combine(context.GetInfrastructureProjectPath(), "Migrations"));

        // Generate .csproj file using relative path
        var csprojContent = GenerateProjectFile(config);
        await context.WriteFileAsync($"src/Infrastructure/{config.MicroserviceName}.Infrastructure.csproj", csprojContent);
    }

    private async Task GenerateDbContextAsync(GenerationContext context, TemplateConfiguration config)
    {
        var dbContextContent = GenerateDbContext(config);
        await context.WriteFileAsync("src/Infrastructure/Persistence/ApplicationDbContext.cs", dbContextContent);
    }

    private async Task GenerateRepositoryAsync(GenerationContext context, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Generate repository interface
        var repositoryInterfaceContent = GenerateRepositoryInterface(config, aggregate);
        await context.WriteFileAsync($"src/Infrastructure/Repositories/I{aggregate.Name}Repository.cs", repositoryInterfaceContent);

        // Generate repository implementation
        var repositoryContent = GenerateRepository(config, aggregate);
        await context.WriteFileAsync($"src/Infrastructure/Repositories/{aggregate.Name}Repository.cs", repositoryContent);
    }

    private async Task GenerateEntityConfigurationAsync(GenerationContext context, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var configurationContent = GenerateEntityConfiguration(config, aggregate);
        await context.WriteFileAsync($"src/Infrastructure/Persistence/Configurations/{aggregate.Name}Configuration.cs", configurationContent);
    }

    private async Task GenerateInfrastructureExtensionsAsync(GenerationContext context, TemplateConfiguration config)
    {
        var extensionsContent = GenerateInfrastructureExtensions(config);
        await context.WriteFileAsync("src/Infrastructure/Extensions/ServiceCollectionExtensions.cs", extensionsContent);
    }

    private async Task GenerateDatabaseConfigurationAsync(GenerationContext context, TemplateConfiguration config)
    {
        var configContent = GenerateDatabaseConfiguration(config);
        await context.WriteFileAsync("src/Infrastructure/Persistence/DatabaseConfiguration.cs", configContent);
    }

    private string GenerateProjectFile(TemplateConfiguration config)
    {
        var provider = config.GetDatabaseProvider();
        
        var packages = new List<string>
        {
            @"<PackageReference Include=""Microsoft.EntityFrameworkCore"" Version=""8.0.10"" />",
            @"<PackageReference Include=""Microsoft.EntityFrameworkCore.Design"" Version=""8.0.10"" />",
            @"<PackageReference Include=""Microsoft.Extensions.Configuration.Abstractions"" Version=""8.0.0"" />",
            @"<PackageReference Include=""Microsoft.Extensions.DependencyInjection.Abstractions"" Version=""8.0.2"" />"
        };

        // Add provider-specific packages
        switch (provider)
        {
            case "postgresql":
                packages.Add(@"<PackageReference Include=""Npgsql.EntityFrameworkCore.PostgreSQL"" Version=""8.0.10"" />");
                break;
            case "sqlserver":
                packages.Add(@"<PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""8.0.10"" />");
                break;
            case "mysql":
                packages.Add(@"<PackageReference Include=""Pomelo.EntityFrameworkCore.MySql"" Version=""8.0.2"" />");
                break;
            case "sqlite":
                packages.Add(@"<PackageReference Include=""Microsoft.EntityFrameworkCore.Sqlite"" Version=""8.0.10"" />");
                break;
            default:
                packages.Add(@"<PackageReference Include=""Microsoft.EntityFrameworkCore.InMemory"" Version=""8.0.10"" />");
                break;
        }

        return $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
{string.Join("\n", packages.Select(p => $"    {p}"))}
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\Domain\{config.MicroserviceName}.Domain.csproj"" />
    <ProjectReference Include=""..\Application\{config.MicroserviceName}.Application.csproj"" />
  </ItemGroup>

</Project>";
    }

    private string GenerateDbContext(TemplateConfiguration config)
    {
        var aggregates = config.Domain?.Aggregates ?? new List<AggregateConfiguration>();
        var dbSets = string.Join("\n    ", aggregates.Select(a => 
            $"public DbSet<{a.Name}> {a.Name}s {{ get; set; }}"));

        var configurations = string.Join("\n        ", aggregates.Select(a => 
            $"modelBuilder.ApplyConfiguration(new {a.Name}Configuration());"));

        return $@"using Microsoft.EntityFrameworkCore;
using {config.Namespace}.Domain.{string.Join($";\nusing {config.Namespace}.Domain.", aggregates.Select(a => a.Name))};
using {config.Namespace}.Infrastructure.Persistence.Configurations;

namespace {config.Namespace}.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {{
    }}

    {dbSets}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        {configurations}

        // Configure domain events (ignore them in database)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {{
            var domainEventsProperty = entityType.ClrType.GetProperty(""DomainEvents"");
            if (domainEventsProperty != null)
            {{
                modelBuilder.Entity(entityType.ClrType).Ignore(""DomainEvents"");
            }}
        }}
    }}

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {{
        // Set audit fields
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {{
            var auditableEntity = (IAuditableEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {{
                auditableEntity.CreatedAt = DateTime.UtcNow;
            }}
            
            if (entry.State == EntityState.Modified)
            {{
                auditableEntity.UpdatedAt = DateTime.UtcNow;
            }}
        }}

        return await base.SaveChangesAsync(cancellationToken);
    }}
}}

public interface IAuditableEntity
{{
    DateTime CreatedAt {{ get; set; }}
    DateTime? UpdatedAt {{ get; set; }}
}}";
    }

    private string GenerateRepositoryInterface(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using {config.Namespace}.Domain.{aggregate.Name};

namespace {config.Namespace}.Infrastructure.Repositories;

public interface I{aggregate.Name}Repository
{{
    Task<{aggregate.Name}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<{aggregate.Name}>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<{aggregate.Name}>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
    Task AddAsync({aggregate.Name} entity, CancellationToken cancellationToken = default);
    Task UpdateAsync({aggregate.Name} entity, CancellationToken cancellationToken = default);
    Task DeleteAsync({aggregate.Name} entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}}";
    }

    private string GenerateRepository(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using Microsoft.EntityFrameworkCore;
using {config.Namespace}.Domain.{aggregate.Name};
using {config.Namespace}.Infrastructure.Persistence;

namespace {config.Namespace}.Infrastructure.Repositories;

public class {aggregate.Name}Repository : I{aggregate.Name}Repository
{{
    private readonly ApplicationDbContext _context;

    public {aggregate.Name}Repository(ApplicationDbContext context)
    {{
        _context = context;
    }}

    public async Task<{aggregate.Name}?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        return await _context.{aggregate.Name}s
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }}

    public async Task<IReadOnlyList<{aggregate.Name}>> GetAllAsync(CancellationToken cancellationToken = default)
    {{
        return await _context.{aggregate.Name}s
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }}

    public async Task<IReadOnlyList<{aggregate.Name}>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {{
        return await _context.{aggregate.Name}s
            .OrderBy(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }}

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {{
        return await _context.{aggregate.Name}s.CountAsync(cancellationToken);
    }}

    public async Task AddAsync({aggregate.Name} entity, CancellationToken cancellationToken = default)
    {{
        await _context.{aggregate.Name}s.AddAsync(entity, cancellationToken);
    }}

    public async Task UpdateAsync({aggregate.Name} entity, CancellationToken cancellationToken = default)
    {{
        _context.{aggregate.Name}s.Update(entity);
    }}

    public async Task DeleteAsync({aggregate.Name} entity, CancellationToken cancellationToken = default)
    {{
        _context.{aggregate.Name}s.Remove(entity);
    }}

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        return await _context.{aggregate.Name}s
            .AnyAsync(x => x.Id == id, cancellationToken);
    }}
}}";
    }

    private string GenerateEntityConfiguration(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var propertyConfigurations = string.Join("\n        ", aggregate.Properties.Select(p => 
            GeneratePropertyConfiguration(p)));

        return $@"using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {config.Namespace}.Domain.{aggregate.Name};

namespace {config.Namespace}.Infrastructure.Persistence.Configurations;

public class {aggregate.Name}Configuration : IEntityTypeConfiguration<{aggregate.Name}>
{{
    public void Configure(EntityTypeBuilder<{aggregate.Name}> builder)
    {{
        builder.ToTable(""{aggregate.Name}s"");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        {propertyConfigurations}

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);
    }}
}}";
    }

    private string GeneratePropertyConfiguration(PropertyConfiguration property)
    {
        return property.Type.ToLowerInvariant() switch
        {
            "string" => $@"builder.Property(x => x.{property.Name})
            .HasMaxLength(255)
            .IsRequired({property.IsRequired.ToString().ToLowerInvariant()});",
            
            "decimal" => $@"builder.Property(x => x.{property.Name})
            .HasPrecision(18, 2)
            .IsRequired({property.IsRequired.ToString().ToLowerInvariant()});",
            
            "datetime" => $@"builder.Property(x => x.{property.Name})
            .IsRequired({property.IsRequired.ToString().ToLowerInvariant()});",
            
            _ => $@"builder.Property(x => x.{property.Name})
            .IsRequired({property.IsRequired.ToString().ToLowerInvariant()});"
        };
    }

    private string GenerateInfrastructureExtensions(TemplateConfiguration config)
    {
        var provider = config.GetDatabaseProvider();
        var dbContextConfiguration = GenerateDbContextConfiguration(provider);
        
        var aggregates = config.Domain?.Aggregates ?? new List<AggregateConfiguration>();
        var repositoryRegistrations = string.Join("\n        ", aggregates.Select(a => 
            $"services.AddScoped<I{a.Name}Repository, {a.Name}Repository>();"));

        return $@"using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using {config.Namespace}.Infrastructure.Persistence;
using {config.Namespace}.Infrastructure.Repositories;

namespace {config.Namespace}.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {{
        // Add DbContext
        {dbContextConfiguration}

        // Add repositories
        {repositoryRegistrations}

        // Add health checks
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        return services;
    }}
}}";
    }

    private string GenerateDbContextConfiguration(string provider)
    {
        return provider switch
        {
            "postgresql" => @"services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString(""DefaultConnection""));
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });",

            "sqlserver" => @"services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString(""DefaultConnection""));
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });",

            "mysql" => @"services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseMySql(configuration.GetConnectionString(""DefaultConnection""), 
                ServerVersion.AutoDetect(configuration.GetConnectionString(""DefaultConnection"")));
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });",

            "sqlite" => @"services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(configuration.GetConnectionString(""DefaultConnection""));
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });",

            _ => @"services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseInMemoryDatabase(""InMemoryDatabase"");
            options.EnableSensitiveDataLogging(true);
        });"
        };
    }

    private string GenerateDatabaseConfiguration(TemplateConfiguration config)
    {
        var provider = config.GetDatabaseProvider();
        var connectionString = GenerateConnectionString(provider);

        return $@"using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using {config.Namespace}.Infrastructure.Persistence;

namespace {config.Namespace}.Infrastructure.Persistence;

public static class DatabaseConfiguration
{{
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {{
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();

        if (environment.IsDevelopment())
        {{
            // Ensure database is created in development
            await context.Database.EnsureCreatedAsync();
        }}
        else
        {{
            // Apply migrations in production
            await context.Database.MigrateAsync();
        }}
    }}

    public static string GetConnectionString(string provider)
    {{
        return provider.ToLowerInvariant() switch
        {{
            ""postgresql"" => ""{connectionString.postgresql}"",
            ""sqlserver"" => ""{connectionString.sqlserver}"",
            ""mysql"" => ""{connectionString.mysql}"",
            ""sqlite"" => ""{connectionString.sqlite}"",
            _ => ""Data Source=:memory:""
        }};
    }}
}}";
    }

    private (string postgresql, string sqlserver, string mysql, string sqlite) GenerateConnectionString(string provider)
    {
        return (
            postgresql: "Host=localhost;Database={ServiceName}Db;Username=postgres;Password=postgres123",
            sqlserver: "Server=localhost;Database={ServiceName}DB;User Id=sa;Password=SqlServer123!;TrustServerCertificate=true",
            mysql: "Server=localhost;Database={ServiceName}DB;User=root;Password=mysql123;",
            sqlite: "Data Source={ServiceName}.db"
        );
    }
}
