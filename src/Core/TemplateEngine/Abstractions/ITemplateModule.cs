using Microservice.Core.TemplateEngine.Configuration;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

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
        var structure = Configuration.ProjectStructure;
        return structure != null 
            ? ResolvePath(structure.DomainProjectPath) 
            : Path.Combine(Configuration.OutputPath, "src/Domain");
    }

    public string GetApplicationProjectPath()
    {
        var structure = Configuration.ProjectStructure;
        return structure != null 
            ? ResolvePath(structure.ApplicationProjectPath) 
            : Path.Combine(Configuration.OutputPath, "src/Application");
    }

    public string GetInfrastructureProjectPath()
    {
        var structure = Configuration.ProjectStructure;
        return structure != null 
            ? ResolvePath(structure.InfrastructureProjectPath) 
            : Path.Combine(Configuration.OutputPath, "src/Infrastructure");
    }

    public string GetApiProjectPath()
    {
        var structure = Configuration.ProjectStructure;
        return structure != null 
            ? ResolvePath(structure.ApiProjectPath) 
            : Path.Combine(Configuration.OutputPath, "src/Api");
    }

    public string GetTestsProjectPath()
    {
        var structure = Configuration.ProjectStructure;
        return structure != null 
            ? ResolvePath(structure.TestsProjectPath) 
            : Path.Combine(Configuration.OutputPath, "tests");
    }

    public string GetIntegrationTestsProjectPath()
    {
        var structure = Configuration.ProjectStructure;
        return structure != null 
            ? ResolvePath(structure.IntegrationTestsProjectPath) 
            : Path.Combine(Configuration.OutputPath, "tests");
    }

    public string GetDockerProjectPath()
    {
        return Path.Combine(Configuration.OutputPath, "docker");
    }

    private string ResolvePath(string pathTemplate)
    {
        var structure = Configuration.ProjectStructure;
        var sourceDir = structure?.SourceDirectory ?? "src";
        
        return pathTemplate
            .Replace("{SourceDirectory}", sourceDir)
            .Replace("{MicroserviceName}", Configuration.MicroserviceName);
    }
}

public record GeneratedFile(string Path, string Content); 