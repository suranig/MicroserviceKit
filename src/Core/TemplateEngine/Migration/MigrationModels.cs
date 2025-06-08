using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Core.TemplateEngine.Migration;

public class MigrationPlan
{
    public ProjectStructure SourceStructure { get; set; } = null!;
    public ArchitectureDecisions TargetStructure { get; set; } = null!;
    public string ProjectPath { get; set; } = string.Empty;
    public List<MigrationStep> Steps { get; set; } = new();
    public TimeSpan EstimatedDuration { get; set; }
    public List<string> Warnings { get; set; } = new();
}

public class MigrationResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public List<MigrationStep> CompletedSteps { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public List<string> GeneratedFiles { get; set; } = new();
    public List<string> ModifiedFiles { get; set; } = new();
}

public class ProjectStructure
{
    public string ServiceName { get; set; } = string.Empty;
    public ArchitectureLevel Level { get; set; }
    public bool HasInfrastructure { get; set; }
    public bool HasDomainLayer { get; set; }
    public bool HasApplicationLayer { get; set; }
    public List<string> Projects { get; set; } = new();
    public List<string> Aggregates { get; set; } = new();
    public List<string> Commands { get; set; } = new();
    public List<string> Queries { get; set; } = new();
    public Dictionary<string, string> Dependencies { get; set; } = new();
}

public abstract record MigrationStep
{
    public string Description { get; init; } = string.Empty;
    public virtual bool IsReversible { get; init; } = true;
    public TimeSpan EstimatedDuration { get; init; } = TimeSpan.FromMinutes(1);
}

public record CreateProjectStep(string ProjectType, string Path) : MigrationStep
{
    public override string ToString() => $"Create {ProjectType} project at {Path}";
}

public record MoveCodeStep(string Pattern, string SourcePath, string TargetPath) : MigrationStep
{
    public override string ToString() => $"Move {Pattern} from {SourcePath} to {TargetPath}";
}

public record UpdateProjectReferencesStep : MigrationStep
{
    public override string ToString() => "Update project references";
}

public record UpdateNamespacesStep : MigrationStep
{
    public override string ToString() => "Update namespaces";
}

public record UpdateSolutionFileStep : MigrationStep
{
    public override string ToString() => "Update solution file";
}

public record DeleteOldProjectStep(string Path) : MigrationStep
{
    public override string ToString() => $"Delete old project at {Path}";
    public override bool IsReversible => false; // Deletion is not easily reversible
}

public record GenerateCodeStep(string CodeType, string TargetPath) : MigrationStep
{
    public override string ToString() => $"Generate {CodeType} at {TargetPath}";
}

public record UpdateConfigurationStep(string ConfigFile, Dictionary<string, object> Changes) : MigrationStep
{
    public override string ToString() => $"Update configuration in {ConfigFile}";
}

public interface IProjectAnalyzer
{
    Task<ProjectStructure> AnalyzeAsync(string projectPath);
}

public interface ICodeMover
{
    Task MoveAsync(string sourcePath, string targetPath, string pattern);
    Task<bool> CanMoveAsync(string sourcePath, string targetPath);
}

public interface ITemplateGenerator
{
    Task GenerateAsync(string templateName, string targetPath, Dictionary<string, object> parameters);
} 