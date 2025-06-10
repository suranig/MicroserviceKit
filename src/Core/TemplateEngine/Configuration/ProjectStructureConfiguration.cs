namespace Microservice.Core.TemplateEngine.Configuration;

public class ProjectStructureConfiguration
{
    public string SourceDirectory { get; set; } = "src";
    public string TestsDirectory { get; set; } = "tests";
    public string DockerDirectory { get; set; } = "docker";
    public string DomainDirectory { get; set; } = "Domain";
    public string ApplicationDirectory { get; set; } = "Application";
    public string InfrastructureDirectory { get; set; } = "Infrastructure";
    public string ApiDirectory { get; set; } = "Api";
    public string CommonDirectory { get; set; } = "Common";
    public string EventsDirectory { get; set; } = "Events";
    public string ExtensionsDirectory { get; set; } = "Extensions";
    public string MessagingDirectory { get; set; } = "Messaging";
    public string PersistenceDirectory { get; set; } = "Persistence";
    public string ControllersDirectory { get; set; } = "Controllers";
    public string MiddlewareDirectory { get; set; } = "Middleware";
    public string ConfigurationDirectory { get; set; } = "Configuration";
    public string RepositoriesDirectory { get; set; } = "Repositories";
    public string ContextsDirectory { get; set; } = "Contexts";
    public string ConfigurationsDirectory { get; set; } = "Configurations";
    public string PublishersDirectory { get; set; } = "Publishers";
    public string SubscribersDirectory { get; set; } = "Subscribers";
    public string ClientsDirectory { get; set; } = "Clients";
    public string BehaviorsDirectory { get; set; } = "Behaviors";
    public string InterfacesDirectory { get; set; } = "Interfaces";
    public string ModelsDirectory { get; set; } = "Models";
    public string UnitTestsDirectory { get; set; } = "Unit";
    public string IntegrationTestsDirectory { get; set; } = "Integration";
    public string EndToEndTestsDirectory { get; set; } = "EndToEnd";
    public string TestFixturesDirectory { get; set; } = "Fixtures";
    public string TestDataDirectory { get; set; } = "Data";
    public string DockerConfigDirectory { get; set; } = "config";

    // Add missing properties from the removed class
    public string DomainProjectPath { get; set; } = "{SourceDirectory}/Domain/{MicroserviceName}.Domain";
    public string ApplicationProjectPath { get; set; } = "{SourceDirectory}/Application/{MicroserviceName}.Application";
    public string InfrastructureProjectPath { get; set; } = "{SourceDirectory}/Infrastructure/{MicroserviceName}.Infrastructure";
    public string ApiProjectPath { get; set; } = "{SourceDirectory}/Api/{MicroserviceName}.Api";
    public string TestsProjectPath { get; set; } = "tests/{MicroserviceName}.Tests";
    public string IntegrationTestsProjectPath { get; set; } = "tests/{MicroserviceName}.IntegrationTests";
} 