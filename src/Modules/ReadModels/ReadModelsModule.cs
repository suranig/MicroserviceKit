using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.ReadModels;

public class ReadModelsModule : ITemplateModule
{
    public string Name => "ReadModels";
    public string Description => "Generates read models in Application layer and MongoDB infrastructure for CQRS read side";

    public bool IsEnabled(TemplateConfiguration config)
    {
        // Enable if we have separate read models or MongoDB is configured
        return config.Features?.Persistence?.ReadModel == "separate" ||
               config.Features?.Database?.ReadModel?.Provider?.ToLowerInvariant() == "mongodb";
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;

        // Generate read models for each aggregate
        if (config.Domain?.Aggregates != null)
        {
            foreach (var aggregate in config.Domain.Aggregates)
            {
                await GenerateReadModelsAsync(context, aggregate);
                await GenerateReadRepositoriesAsync(context, aggregate);
            }
        }

        // Generate MongoDB infrastructure if configured
        if (config.Features?.Database?.ReadModel?.Provider?.ToLowerInvariant() == "mongodb")
        {
            await GenerateMongoDbInfrastructureAsync(context);
        }

        // Generate read model extensions
        await GenerateReadModelExtensionsAsync(context);
    }

    private async Task GenerateReadModelsAsync(GenerationContext context, AggregateConfiguration aggregate)
    {
        var config = context.Configuration;
        var applicationPath = Path.Combine(config.OutputPath, "src", "Application", $"{config.MicroserviceName}.Application");

        // Create read models directory
        var readModelsPath = Path.Combine(applicationPath, aggregate.Name, "ReadModels");
        Directory.CreateDirectory(readModelsPath);

        // Generate main read model
        var readModelContent = GenerateReadModel(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(readModelsPath, $"{aggregate.Name}ReadModel.cs"),
            readModelContent);

        // Generate list read model (for queries)
        var listReadModelContent = GenerateListReadModel(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(readModelsPath, $"{aggregate.Name}ListReadModel.cs"),
            listReadModelContent);

        // Generate summary read model (for dashboards)
        var summaryReadModelContent = GenerateSummaryReadModel(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(readModelsPath, $"{aggregate.Name}SummaryReadModel.cs"),
            summaryReadModelContent);
    }

    private async Task GenerateReadRepositoriesAsync(GenerationContext context, AggregateConfiguration aggregate)
    {
        var config = context.Configuration;
        var applicationPath = Path.Combine(config.OutputPath, "src", "Application", $"{config.MicroserviceName}.Application");

        // Create repositories directory
        var repositoriesPath = Path.Combine(applicationPath, aggregate.Name, "Repositories");
        Directory.CreateDirectory(repositoriesPath);

        // Generate read repository interface
        var readRepositoryInterface = GenerateReadRepositoryInterface(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(repositoriesPath, $"I{aggregate.Name}ReadRepository.cs"),
            readRepositoryInterface);

        // Generate read repository implementation (will be in Infrastructure)
        var infrastructurePath = Path.Combine(config.OutputPath, "src", "Infrastructure", $"{config.MicroserviceName}.Infrastructure");
        Directory.CreateDirectory(Path.Combine(infrastructurePath, "ReadModels", "Repositories"));

        var readRepositoryImplementation = GenerateReadRepositoryImplementation(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "ReadModels", "Repositories", $"{aggregate.Name}ReadRepository.cs"),
            readRepositoryImplementation);
    }

    private async Task GenerateMongoDbInfrastructureAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var infrastructurePath = Path.Combine(config.OutputPath, "src", "Infrastructure", $"{config.MicroserviceName}.Infrastructure");

        // Create MongoDB directories
        Directory.CreateDirectory(Path.Combine(infrastructurePath, "ReadModels"));
        Directory.CreateDirectory(Path.Combine(infrastructurePath, "ReadModels", "Configuration"));

        // Generate MongoDB context
        var mongoContextContent = GenerateMongoDbContext(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "ReadModels", "MongoDbContext.cs"),
            mongoContextContent);

        // Generate MongoDB configuration
        var mongoConfigContent = GenerateMongoDbConfiguration(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "ReadModels", "Configuration", "MongoDbConfiguration.cs"),
            mongoConfigContent);

        // Generate MongoDB extensions
        var mongoExtensionsContent = GenerateMongoDbExtensions(config);
        await File.WriteAllTextAsync(
            Path.Combine(infrastructurePath, "Extensions", "MongoDbExtensions.cs"),
            mongoExtensionsContent);
    }

    private async Task GenerateReadModelExtensionsAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var applicationPath = Path.Combine(config.OutputPath, "src", "Application", $"{config.MicroserviceName}.Application");

        // Generate read model extensions
        var readModelExtensions = GenerateReadModelServiceExtensions(config);
        await File.WriteAllTextAsync(
            Path.Combine(applicationPath, "Extensions", "ReadModelExtensions.cs"),
            readModelExtensions);
    }

    private string GenerateReadModel(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var properties = string.Join("\n    ", aggregate.Properties.Select(p => 
            $"public {p.Type} {p.Name} {{ get; set; }}"));

        return $@"using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace {config.Namespace}.Application.{aggregate.Name}.ReadModels;

/// <summary>
/// Read model for {aggregate.Name} - optimized for queries
/// </summary>
public class {aggregate.Name}ReadModel
{{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id {{ get; set; }}

    {properties}

    // Audit fields
    public DateTime CreatedAt {{ get; set; }}
    public DateTime? UpdatedAt {{ get; set; }}

    // Denormalized fields for query optimization
    public string SearchText {{ get; set; }} = string.Empty;
    public List<string> Tags {{ get; set; }} = new();
    
    // Version for optimistic concurrency
    public long Version {{ get; set; }}

    // Metadata
    public Dictionary<string, object> Metadata {{ get; set; }} = new();
}}";
    }

    private string GenerateListReadModel(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Get key properties for list view (first 3 properties)
        var keyProperties = aggregate.Properties.Take(3).ToList();
        var properties = string.Join("\n    ", keyProperties.Select(p => 
            $"public {p.Type} {p.Name} {{ get; set; }}"));

        return $@"using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace {config.Namespace}.Application.{aggregate.Name}.ReadModels;

/// <summary>
/// Lightweight read model for {aggregate.Name} lists and grids
/// </summary>
public class {aggregate.Name}ListReadModel
{{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id {{ get; set; }}

    {properties}

    public DateTime CreatedAt {{ get; set; }}
    public DateTime? UpdatedAt {{ get; set; }}

    // Status for filtering
    public string Status {{ get; set; }} = ""Active"";
    
    // Search optimization
    public string DisplayName {{ get; set; }} = string.Empty;
    public string SearchText {{ get; set; }} = string.Empty;
}}";
    }

    private string GenerateSummaryReadModel(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace {config.Namespace}.Application.{aggregate.Name}.ReadModels;

/// <summary>
/// Summary read model for {aggregate.Name} - for dashboards and analytics
/// </summary>
public class {aggregate.Name}SummaryReadModel
{{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id {{ get; set; }} = string.Empty; // e.g., ""daily-2024-01-15""

    public DateTime PeriodStart {{ get; set; }}
    public DateTime PeriodEnd {{ get; set; }}
    public string PeriodType {{ get; set; }} = ""Daily""; // Daily, Weekly, Monthly

    // Aggregated metrics
    public int TotalCount {{ get; set; }}
    public int ActiveCount {{ get; set; }}
    public int InactiveCount {{ get; set; }}

    // Additional metrics based on aggregate properties
    public Dictionary<string, int> StatusCounts {{ get; set; }} = new();
    public Dictionary<string, decimal> Totals {{ get; set; }} = new();
    public Dictionary<string, double> Averages {{ get; set; }} = new();

    public DateTime LastUpdated {{ get; set; }} = DateTime.UtcNow;
}}";
    }

    private string GenerateReadRepositoryInterface(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using {config.Namespace}.Application.{aggregate.Name}.ReadModels;
using {config.Namespace}.Application.Common.Models;

namespace {config.Namespace}.Application.{aggregate.Name}.Repositories;

public interface I{aggregate.Name}ReadRepository
{{
    // Single item queries
    Task<{aggregate.Name}ReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<{aggregate.Name}ListReadModel?> GetListItemByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // List queries
    Task<PagedResult<{aggregate.Name}ListReadModel>> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        string? status = null,
        CancellationToken cancellationToken = default);

    Task<List<{aggregate.Name}ListReadModel>> GetByIdsAsync(
        IEnumerable<Guid> ids, 
        CancellationToken cancellationToken = default);

    // Search queries
    Task<List<{aggregate.Name}ListReadModel>> SearchAsync(
        string searchTerm, 
        int maxResults = 50,
        CancellationToken cancellationToken = default);

    // Summary queries
    Task<{aggregate.Name}SummaryReadModel?> GetSummaryAsync(
        DateTime periodStart,
        DateTime periodEnd,
        string periodType = ""Daily"",
        CancellationToken cancellationToken = default);

    // Write operations (for event handlers)
    Task CreateAsync({aggregate.Name}ReadModel readModel, CancellationToken cancellationToken = default);
    Task UpdateAsync({aggregate.Name}ReadModel readModel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // Bulk operations
    Task CreateManyAsync(IEnumerable<{aggregate.Name}ReadModel> readModels, CancellationToken cancellationToken = default);
    Task UpdateManyAsync(IEnumerable<{aggregate.Name}ReadModel> readModels, CancellationToken cancellationToken = default);

    // Maintenance
    Task RebuildSearchIndexAsync(CancellationToken cancellationToken = default);
    Task<long> GetCountAsync(CancellationToken cancellationToken = default);
}}";
    }

    private string GenerateReadRepositoryImplementation(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using MongoDB.Driver;
using {config.Namespace}.Application.{aggregate.Name}.ReadModels;
using {config.Namespace}.Application.{aggregate.Name}.Repositories;
using {config.Namespace}.Application.Common.Models;
using {config.Namespace}.Infrastructure.ReadModels;

namespace {config.Namespace}.Infrastructure.ReadModels.Repositories;

public class {aggregate.Name}ReadRepository : I{aggregate.Name}ReadRepository
{{
    private readonly IMongoCollection<{aggregate.Name}ReadModel> _collection;
    private readonly IMongoCollection<{aggregate.Name}ListReadModel> _listCollection;
    private readonly IMongoCollection<{aggregate.Name}SummaryReadModel> _summaryCollection;

    public {aggregate.Name}ReadRepository(MongoDbContext context)
    {{
        _collection = context.GetCollection<{aggregate.Name}ReadModel>(""{aggregate.Name.ToLowerInvariant()}s"");
        _listCollection = context.GetCollection<{aggregate.Name}ListReadModel>(""{aggregate.Name.ToLowerInvariant()}s_list"");
        _summaryCollection = context.GetCollection<{aggregate.Name}SummaryReadModel>(""{aggregate.Name.ToLowerInvariant()}s_summary"");
    }}

    public async Task<{aggregate.Name}ReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        return await _collection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }}

    public async Task<{aggregate.Name}ListReadModel?> GetListItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        return await _listCollection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }}

    public async Task<PagedResult<{aggregate.Name}ListReadModel>> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {{
        var filterBuilder = Builders<{aggregate.Name}ListReadModel>.Filter;
        var filter = filterBuilder.Empty;

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {{
            filter &= filterBuilder.Text(searchTerm);
        }}

        if (!string.IsNullOrWhiteSpace(status))
        {{
            filter &= filterBuilder.Eq(x => x.Status, status);
        }}

        var totalCount = await _listCollection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        
        var items = await _listCollection
            .Find(filter)
            .Sort(Builders<{aggregate.Name}ListReadModel>.Sort.Descending(x => x.CreatedAt))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<{aggregate.Name}ListReadModel>
        {{
            Items = items,
            TotalCount = (int)totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        }};
    }}

    public async Task<List<{aggregate.Name}ListReadModel>> GetByIdsAsync(
        IEnumerable<Guid> ids, 
        CancellationToken cancellationToken = default)
    {{
        return await _listCollection
            .Find(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }}

    public async Task<List<{aggregate.Name}ListReadModel>> SearchAsync(
        string searchTerm, 
        int maxResults = 50,
        CancellationToken cancellationToken = default)
    {{
        return await _listCollection
            .Find(Builders<{aggregate.Name}ListReadModel>.Filter.Text(searchTerm))
            .Limit(maxResults)
            .ToListAsync(cancellationToken);
    }}

    public async Task<{aggregate.Name}SummaryReadModel?> GetSummaryAsync(
        DateTime periodStart,
        DateTime periodEnd,
        string periodType = ""Daily"",
        CancellationToken cancellationToken = default)
    {{
        var id = $""{{periodType.ToLowerInvariant()}}-{{periodStart:yyyy-MM-dd}}"";
        return await _summaryCollection
            .Find(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }}

    public async Task CreateAsync({aggregate.Name}ReadModel readModel, CancellationToken cancellationToken = default)
    {{
        readModel.CreatedAt = DateTime.UtcNow;
        readModel.Version = 1;
        
        await _collection.InsertOneAsync(readModel, cancellationToken: cancellationToken);
        
        // Also create list item
        var listItem = MapToListReadModel(readModel);
        await _listCollection.InsertOneAsync(listItem, cancellationToken: cancellationToken);
    }}

    public async Task UpdateAsync({aggregate.Name}ReadModel readModel, CancellationToken cancellationToken = default)
    {{
        readModel.UpdatedAt = DateTime.UtcNow;
        readModel.Version++;
        
        await _collection.ReplaceOneAsync(
            x => x.Id == readModel.Id,
            readModel,
            cancellationToken: cancellationToken);
            
        // Also update list item
        var listItem = MapToListReadModel(readModel);
        await _listCollection.ReplaceOneAsync(
            x => x.Id == readModel.Id,
            listItem,
            cancellationToken: cancellationToken);
    }}

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {{
        await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken);
        await _listCollection.DeleteOneAsync(x => x.Id == id, cancellationToken);
    }}

    public async Task CreateManyAsync(IEnumerable<{aggregate.Name}ReadModel> readModels, CancellationToken cancellationToken = default)
    {{
        var models = readModels.ToList();
        foreach (var model in models)
        {{
            model.CreatedAt = DateTime.UtcNow;
            model.Version = 1;
        }}
        
        await _collection.InsertManyAsync(models, cancellationToken: cancellationToken);
        
        var listItems = models.Select(MapToListReadModel);
        await _listCollection.InsertManyAsync(listItems, cancellationToken: cancellationToken);
    }}

    public async Task UpdateManyAsync(IEnumerable<{aggregate.Name}ReadModel> readModels, CancellationToken cancellationToken = default)
    {{
        var models = readModels.ToList();
        foreach (var model in models)
        {{
            model.UpdatedAt = DateTime.UtcNow;
            model.Version++;
        }}

        var bulkOps = models.Select(model =>
            new ReplaceOneModel<{aggregate.Name}ReadModel>(
                Builders<{aggregate.Name}ReadModel>.Filter.Eq(x => x.Id, model.Id),
                model));

        await _collection.BulkWriteAsync(bulkOps, cancellationToken: cancellationToken);
    }}

    public async Task RebuildSearchIndexAsync(CancellationToken cancellationToken = default)
    {{
        // Rebuild search text for all documents
        var cursor = await _collection.Find(Builders<{aggregate.Name}ReadModel>.Filter.Empty)
            .ToCursorAsync(cancellationToken);

        while (await cursor.MoveNextAsync(cancellationToken))
        {{
            var batch = cursor.Current;
            var updates = new List<WriteModel<{aggregate.Name}ReadModel>>();

            foreach (var item in batch)
            {{
                item.SearchText = BuildSearchText(item);
                updates.Add(new ReplaceOneModel<{aggregate.Name}ReadModel>(
                    Builders<{aggregate.Name}ReadModel>.Filter.Eq(x => x.Id, item.Id),
                    item));
            }}

            if (updates.Any())
            {{
                await _collection.BulkWriteAsync(updates, cancellationToken: cancellationToken);
            }}
        }}
    }}

    public async Task<long> GetCountAsync(CancellationToken cancellationToken = default)
    {{
        return await _collection.CountDocumentsAsync(
            Builders<{aggregate.Name}ReadModel>.Filter.Empty, 
            cancellationToken: cancellationToken);
    }}

    private {aggregate.Name}ListReadModel MapToListReadModel({aggregate.Name}ReadModel readModel)
    {{
        return new {aggregate.Name}ListReadModel
        {{
            Id = readModel.Id,
{string.Join(",\n", aggregate.Properties.Take(3).Select(p => $"            {p.Name} = readModel.{p.Name}"))}
            CreatedAt = readModel.CreatedAt,
            UpdatedAt = readModel.UpdatedAt,
            Status = ""Active"", // TODO: Map from actual status
            DisplayName = $""{{readModel.{aggregate.Properties.FirstOrDefault()?.Name ?? "Id"}}}"",
            SearchText = BuildSearchText(readModel)
        }};
    }}

    private string BuildSearchText({aggregate.Name}ReadModel readModel)
    {{
        var searchParts = new List<string>();
        
{string.Join("\n", aggregate.Properties.Where(p => p.Type == "string").Select(p => $"        if (!string.IsNullOrWhiteSpace(readModel.{p.Name})) searchParts.Add(readModel.{p.Name});"))}
        
        return string.Join("" "", searchParts).ToLowerInvariant();
    }}
}}";
    }

    private string GenerateMongoDbContext(TemplateConfiguration config)
    {
        return $@"using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace {config.Namespace}.Infrastructure.ReadModels;

public class MongoDbContext
{{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {{
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }}

    public IMongoCollection<T> GetCollection<T>(string name)
    {{
        return _database.GetCollection<T>(name);
    }}

    public IMongoDatabase Database => _database;
}}

public class MongoDbSettings
{{
    public string ConnectionString {{ get; set; }} = ""mongodb://localhost:27017"";
    public string DatabaseName {{ get; set; }} = ""{config.MicroserviceName.ToLowerInvariant()}_readmodels"";
}}";
    }

    private string GenerateMongoDbConfiguration(TemplateConfiguration config)
    {
        return $@"using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace {config.Namespace}.Infrastructure.ReadModels.Configuration;

public static class MongoDbConfiguration
{{
    public static void Configure()
    {{
        // Register conventions
        var conventionPack = new ConventionPack
        {{
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
            new IgnoreIfDefaultConvention(true)
        }};
        
        ConventionRegistry.Register(""camelCase"", conventionPack, t => true);

        // Register serializers
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        BsonSerializer.RegisterSerializer(new DateTimeSerializer(BsonType.DateTime));
        BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));

        // Configure class maps if needed
        ConfigureClassMaps();
    }}

    private static void ConfigureClassMaps()
    {{
        // Add custom class maps here if needed
        // Example:
        // if (!BsonClassMap.IsClassMapRegistered(typeof(SomeClass)))
        // {{
        //     BsonClassMap.RegisterClassMap<SomeClass>(cm =>
        //     {{
        //         cm.AutoMap();
        //         cm.SetIgnoreExtraElements(true);
        //     }});
        // }}
    }}
}}";
    }

    private string GenerateMongoDbExtensions(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using {config.Namespace}.Infrastructure.ReadModels;
using {config.Namespace}.Infrastructure.ReadModels.Configuration;
using {config.Namespace}.Infrastructure.ReadModels.Repositories;
using {config.Namespace}.Application.Common.Models;

namespace {config.Namespace}.Infrastructure.Extensions;

public static class MongoDbExtensions
{{
    public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {{
        // Configure MongoDB
        MongoDbConfiguration.Configure();

        // Register settings
        services.Configure<MongoDbSettings>(configuration.GetSection(""MongoDB""));

        // Register context
        services.AddSingleton<MongoDbContext>();

        // Register read repositories
        RegisterReadRepositories(services);

        // Add health checks
        services.AddHealthChecks()
            .AddMongoDb(
                configuration.GetConnectionString(""MongoDB"") ?? ""mongodb://localhost:27017"",
                name: ""mongodb"",
                tags: new[] {{ ""ready"" }});

        return services;
    }}

    private static void RegisterReadRepositories(IServiceCollection services)
    {{
        // Auto-register all read repositories
        var infrastructureAssembly = typeof({config.Namespace}.Infrastructure.AssemblyReference).Assembly;
        
        var repositoryTypes = infrastructureAssembly.GetTypes()
            .Where(t => t.Name.EndsWith(""ReadRepository"") && !t.IsAbstract && !t.IsInterface)
            .ToList();

        foreach (var repositoryType in repositoryTypes)
        {{
            var interfaceType = repositoryType.GetInterfaces()
                .FirstOrDefault(i => i.Name == $""I{{repositoryType.Name}}"");

            if (interfaceType != null)
            {{
                services.AddScoped(interfaceType, repositoryType);
            }}
        }}
    }}
}}";
    }

    private string GenerateReadModelServiceExtensions(TemplateConfiguration config)
    {
        return $@"using Microsoft.Extensions.DependencyInjection;

namespace {config.Namespace}.Application.Extensions;

public static class ReadModelExtensions
{{
    public static IServiceCollection AddReadModels(this IServiceCollection services)
    {{
        // Read model services will be registered automatically
        // through the infrastructure layer
        
        return services;
    }}
}}";
    }
} 