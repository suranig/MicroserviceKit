namespace Microservice.Core.TemplateEngine.Configuration;

public static class ArchitectureRules
{
    public static ArchitectureDecisions MakeDecisions(TemplateConfiguration config)
    {
        var decisions = new ArchitectureDecisions();
        
        // Określ poziom złożoności na podstawie domeny
        var complexity = CalculateComplexity(config);
        decisions.ArchitectureLevel = DetermineArchitectureLevel(config.Architecture?.Level, complexity);
        
        // Automatyczne decyzje na podstawie poziomu
        decisions.ProjectStructure = DetermineProjectStructure(decisions.ArchitectureLevel);
        decisions.EnableDDD = ShouldEnableDDD(config, complexity);
        decisions.EnableCQRS = ShouldEnableCQRS(config, complexity);
        decisions.ApiStyle = DetermineApiStyle(config, decisions.ArchitectureLevel);
        decisions.PersistenceStrategy = DeterminePersistence(config);
        decisions.EnableDocker = ShouldEnableDocker(config, decisions.ArchitectureLevel);
        decisions.EnableInfrastructure = ShouldEnableInfrastructure(config, decisions.ArchitectureLevel);
        
        return decisions;
    }
    
    private static ComplexityLevel CalculateComplexity(TemplateConfiguration config)
    {
        var score = 0;
        
        // Punkty za agregaty
        score += (config.Domain?.Aggregates?.Count ?? 0) * 2;
        
        // Punkty za operacje
        var totalOperations = config.Domain?.Aggregates?.Sum(a => a.Operations?.Count ?? 0) ?? 0;
        score += totalOperations;
        
        // Punkty za value objects
        score += (config.Domain?.ValueObjects?.Count ?? 0);
        
        // Punkty za messaging
        if (config.Features?.Messaging?.Enabled == true) score += 3;
        
        // Punkty za authentication
        if (config.Features?.Api?.Authentication != "none") score += 2;
        
        return score switch
        {
            <= 3 => ComplexityLevel.Simple,
            <= 8 => ComplexityLevel.Medium,
            _ => ComplexityLevel.Complex
        };
    }
    
    private static ArchitectureLevel DetermineArchitectureLevel(string? requested, ComplexityLevel complexity)
    {
        return requested switch
        {
            "minimal" => ArchitectureLevel.Minimal,
            "standard" => ArchitectureLevel.Standard,
            "enterprise" => ArchitectureLevel.Enterprise,
            _ => complexity switch // auto
            {
                ComplexityLevel.Simple => ArchitectureLevel.Minimal,
                ComplexityLevel.Medium => ArchitectureLevel.Standard,
                ComplexityLevel.Complex => ArchitectureLevel.Enterprise,
                _ => ArchitectureLevel.Standard // default
            }
        };
    }
    
    private static ProjectStructure DetermineProjectStructure(ArchitectureLevel level)
    {
        return level switch
        {
            ArchitectureLevel.Minimal => ProjectStructure.SingleProject,
            ArchitectureLevel.Standard => ProjectStructure.ThreeLayer,
            ArchitectureLevel.Enterprise => ProjectStructure.FourLayer,
            _ => ProjectStructure.ThreeLayer // default
        };
    }
    
    private static bool ShouldEnableDDD(TemplateConfiguration config, ComplexityLevel complexity)
    {
        return config.Architecture?.Patterns?.DDD switch
        {
            "enabled" => true,
            "disabled" => false,
            _ => complexity >= ComplexityLevel.Medium // auto
        };
    }
    
    private static bool ShouldEnableCQRS(TemplateConfiguration config, ComplexityLevel complexity)
    {
        return config.Architecture?.Patterns?.CQRS switch
        {
            "enabled" => true,
            "disabled" => false,
            _ => complexity >= ComplexityLevel.Medium || 
                 (config.Domain?.Aggregates?.Any(a => (a.Operations?.Count ?? 0) > 2) ?? false) // auto
        };
    }
    
    private static ApiStyle DetermineApiStyle(TemplateConfiguration config, ArchitectureLevel level)
    {
        return config.Features?.Api?.Style switch
        {
            "minimal" => ApiStyle.MinimalApi,
            "controllers" => ApiStyle.Controllers,
            "both" => ApiStyle.Both,
            _ => level switch // auto
            {
                ArchitectureLevel.Minimal => ApiStyle.MinimalApi,
                _ => ApiStyle.Controllers
            }
        };
    }
    
    private static PersistenceStrategy DeterminePersistence(TemplateConfiguration config)
    {
        var writeProvider = config.GetDatabaseProvider();
        var readProvider = config.GetReadModelProvider();
        
        return new PersistenceStrategy
        {
            WriteProvider = writeProvider,
            ReadProvider = readProvider,
            SeparateReadModel = readProvider != writeProvider
        };
    }
    
    private static bool ShouldEnableDocker(TemplateConfiguration config, ArchitectureLevel level)
    {
        return config.Features?.Deployment?.Docker switch
        {
            "enabled" => true,
            "disabled" => false,
            _ => level >= ArchitectureLevel.Standard // auto
        };
    }
    
    private static bool ShouldEnableInfrastructure(TemplateConfiguration config, ArchitectureLevel level)
    {
        // Infrastructure layer potrzebny gdy:
        var hasExternalServices = config.Features?.ExternalServices?.Enabled == true;
        var hasMessaging = config.Features?.Messaging?.Enabled == true;
        var hasBackgroundJobs = config.Features?.BackgroundJobs?.Enabled == true;
        var hasCaching = config.Features?.Database?.Cache?.Enabled == true;
        var hasSeparateReadModel = config.Features?.Database?.ReadModel?.Provider != "same";
        var hasComplexPersistence = config.Features?.Database?.EventStore?.Enabled == true;
        var hasDatabase = config.GetDatabaseProvider() != "inmemory";
        
        return level switch
        {
            ArchitectureLevel.Minimal => false, // Nigdy dla minimal
            ArchitectureLevel.Standard => true, // Zawsze dla standard (potrzebne dla repositories)
            ArchitectureLevel.Enterprise => true, // Zawsze dla enterprise
            _ => false
        };
    }
}

public class ArchitectureDecisions
{
    public ArchitectureLevel ArchitectureLevel { get; set; }
    public ProjectStructure ProjectStructure { get; set; }
    public bool EnableDDD { get; set; }
    public bool EnableCQRS { get; set; }
    public ApiStyle ApiStyle { get; set; }
    public PersistenceStrategy PersistenceStrategy { get; set; } = new();
    public bool EnableDocker { get; set; }
    public bool EnableInfrastructure { get; set; }
}

public enum ComplexityLevel { Simple, Medium, Complex }
public enum ArchitectureLevel { Minimal, Standard, Enterprise }
public enum ProjectStructure { SingleProject, ThreeLayer, FourLayer }
public enum ApiStyle { MinimalApi, Controllers, Both }

public class PersistenceStrategy
{
    public string WriteProvider { get; set; } = "inmemory";
    public string ReadProvider { get; set; } = "inmemory";
    public bool SeparateReadModel { get; set; }
} 