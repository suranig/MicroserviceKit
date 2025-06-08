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
}

public record GeneratedFile(string Path, string Content); 