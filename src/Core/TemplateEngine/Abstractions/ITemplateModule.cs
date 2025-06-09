using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Core.TemplateEngine.Abstractions;

public interface ITemplateModule
{
    string Name { get; }
    bool IsEnabled(TemplateConfiguration config);
    Task GenerateAsync(GenerationContext context);
}

public class GenerationContext
{
    public TemplateConfiguration Configuration { get; }
    public List<GeneratedFile> GeneratedFiles { get; } = new();
    
    public GenerationContext(TemplateConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    public async Task WriteFileAsync(string relativePath, string content)
    {
        var fullPath = Path.Combine(Configuration.OutputPath, relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        await File.WriteAllTextAsync(fullPath, content);
        GeneratedFiles.Add(new GeneratedFile(relativePath, content));
    }

    // Helper methods for generating project paths
    public string GetDomainProjectPath()
    {
        var structure = Configuration.ProjectStructure ?? new ProjectStructureConfiguration();
        return ResolvePath(structure.DomainProjectPath);
    }

    public string GetApplicationProjectPath()
    {
        var structure = Configuration.ProjectStructure ?? new ProjectStructureConfiguration();
        return ResolvePath(structure.ApplicationProjectPath);
    }

    public string GetInfrastructureProjectPath()
    {
        var structure = Configuration.ProjectStructure ?? new ProjectStructureConfiguration();
        return ResolvePath(structure.InfrastructureProjectPath);
    }

    public string GetApiProjectPath()
    {
        var structure = Configuration.ProjectStructure ?? new ProjectStructureConfiguration();
        return ResolvePath(structure.ApiProjectPath);
    }

    public string GetTestsProjectPath()
    {
        var structure = Configuration.ProjectStructure ?? new ProjectStructureConfiguration();
        return ResolvePath(structure.TestsProjectPath);
    }

    public string GetIntegrationTestsProjectPath()
    {
        var structure = Configuration.ProjectStructure ?? new ProjectStructureConfiguration();
        return ResolvePath(structure.IntegrationTestsProjectPath);
    }

    private string ResolvePath(string pathTemplate)
    {
        var structure = Configuration.ProjectStructure ?? new ProjectStructureConfiguration();
        return pathTemplate
            .Replace("{SourceDirectory}", structure.SourceDirectory)
            .Replace("{MicroserviceName}", Configuration.MicroserviceName);
    }
}

public record GeneratedFile(string Path, string Content); 