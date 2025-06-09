using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microservice.Core.TemplateEngine.Migration;

public class MigrationHistory
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMigrationAt { get; set; } = DateTime.UtcNow;
    public string CurrentLevel { get; set; } = "minimal";
    public List<MigrationRecord> Migrations { get; set; } = new();
    public ProjectSnapshot CurrentSnapshot { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class MigrationRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string FromLevel { get; set; } = string.Empty;
    public string ToLevel { get; set; } = string.Empty;
    public string Status { get; set; } = "completed"; // completed | failed | rolled_back
    public TimeSpan Duration { get; set; }
    public List<ExecutedStep> ExecutedSteps { get; set; } = new();
    public ProjectSnapshot BeforeSnapshot { get; set; } = new();
    public ProjectSnapshot AfterSnapshot { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? ConfigurationUsed { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ExecutedStep
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan Duration { get; set; }
    public string Status { get; set; } = "completed"; // completed | failed | skipped
    public List<string> FilesCreated { get; set; } = new();
    public List<string> FilesModified { get; set; } = new();
    public List<string> FilesDeleted { get; set; } = new();
    public Dictionary<string, object> Details { get; set; } = new();
}

public class ProjectSnapshot
{
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    public string ArchitectureLevel { get; set; } = string.Empty;
    public List<ProjectInfo> Projects { get; set; } = new();
    public List<string> Aggregates { get; set; } = new();
    public List<string> Commands { get; set; } = new();
    public List<string> Queries { get; set; } = new();
    public List<string> ExternalServices { get; set; } = new();
    public Dictionary<string, string> Dependencies { get; set; } = new();
    public Dictionary<string, object> Features { get; set; } = new();
    public string GitCommit { get; set; } = string.Empty;
    public List<string> CustomFiles { get; set; } = new(); // User-modified files to preserve
}

public class ProjectInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Domain | Application | Api | Infrastructure
    public List<string> References { get; set; } = new();
    public List<string> Packages { get; set; } = new();
    public int FileCount { get; set; }
    public long SizeBytes { get; set; }
}

public interface IMigrationHistoryService
{
    Task<MigrationHistory> LoadHistoryAsync(string projectPath);
    Task SaveHistoryAsync(string projectPath, MigrationHistory history);
    Task<ProjectSnapshot> CaptureSnapshotAsync(string projectPath);
    Task<bool> CanMigrateToAsync(string projectPath, string targetLevel);
    Task<List<string>> GetMigrationPathAsync(string currentLevel, string targetLevel);
    Task RecordMigrationStartAsync(string projectPath, string fromLevel, string toLevel, string? configPath = null);
    Task RecordMigrationStepAsync(string projectPath, ExecutedStep step);
    Task RecordMigrationCompleteAsync(string projectPath, bool success, string? errorMessage = null);
}

public class MigrationHistoryService : IMigrationHistoryService
{
    private const string HistoryFileName = ".microservice-history.json";
    private readonly JsonSerializerOptions _jsonOptions;

    public MigrationHistoryService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<MigrationHistory> LoadHistoryAsync(string projectPath)
    {
        var historyPath = Path.Combine(projectPath, HistoryFileName);
        
        if (!File.Exists(historyPath))
        {
            // Create initial history
            var initialHistory = new MigrationHistory
            {
                ServiceName = Path.GetFileName(projectPath),
                CurrentSnapshot = await CaptureSnapshotAsync(projectPath)
            };
            
            await SaveHistoryAsync(projectPath, initialHistory);
            return initialHistory;
        }

        var json = await File.ReadAllTextAsync(historyPath);
        return JsonSerializer.Deserialize<MigrationHistory>(json, _jsonOptions) 
               ?? throw new InvalidOperationException("Failed to deserialize migration history");
    }

    public async Task SaveHistoryAsync(string projectPath, MigrationHistory history)
    {
        var historyPath = Path.Combine(projectPath, HistoryFileName);
        history.LastMigrationAt = DateTime.UtcNow;
        
        var json = JsonSerializer.Serialize(history, _jsonOptions);
        await File.WriteAllTextAsync(historyPath, json);
    }

    public async Task<ProjectSnapshot> CaptureSnapshotAsync(string projectPath)
    {
        var snapshot = new ProjectSnapshot
        {
            CapturedAt = DateTime.UtcNow,
            GitCommit = await GetGitCommitAsync(projectPath)
        };

        // Analyze project structure
        var projects = new List<ProjectInfo>();
        var srcPath = Path.Combine(projectPath, "src");
        
        if (Directory.Exists(srcPath))
        {
            foreach (var projectDir in Directory.GetDirectories(srcPath, "*", SearchOption.AllDirectories))
            {
                var csprojFiles = Directory.GetFiles(projectDir, "*.csproj");
                if (csprojFiles.Length > 0)
                {
                    var projectInfo = await AnalyzeProjectAsync(projectDir, csprojFiles[0]);
                    projects.Add(projectInfo);
                }
            }
        }

        snapshot.Projects = projects;
        
        // Determine architecture level
        snapshot.ArchitectureLevel = DetermineArchitectureLevel(projects);
        
        // Scan for domain elements
        await ScanDomainElementsAsync(projectPath, snapshot);
        
        return snapshot;
    }

    public async Task<bool> CanMigrateToAsync(string projectPath, string targetLevel)
    {
        var history = await LoadHistoryAsync(projectPath);
        var currentLevel = history.CurrentLevel;
        
        // Can always migrate up, but not down (for safety)
        var levels = new[] { "minimal", "standard", "enterprise" };
        var currentIndex = Array.IndexOf(levels, currentLevel);
        var targetIndex = Array.IndexOf(levels, targetLevel);
        
        return targetIndex >= currentIndex;
    }

    public Task<List<string>> GetMigrationPathAsync(string currentLevel, string targetLevel)
    {
        var levels = new[] { "minimal", "standard", "enterprise" };
        var currentIndex = Array.IndexOf(levels, currentLevel);
        var targetIndex = Array.IndexOf(levels, targetLevel);
        
        var path = new List<string>();
        for (int i = currentIndex + 1; i <= targetIndex; i++)
        {
            path.Add(levels[i]);
        }
        
        return Task.FromResult(path);
    }

    public async Task RecordMigrationStartAsync(string projectPath, string fromLevel, string toLevel, string? configPath = null)
    {
        var history = await LoadHistoryAsync(projectPath);
        
        var migration = new MigrationRecord
        {
            FromLevel = fromLevel,
            ToLevel = toLevel,
            BeforeSnapshot = await CaptureSnapshotAsync(projectPath),
            Status = "in_progress",
            ConfigurationUsed = configPath != null ? await File.ReadAllTextAsync(configPath) : null
        };
        
        history.Migrations.Add(migration);
        await SaveHistoryAsync(projectPath, history);
    }

    public async Task RecordMigrationStepAsync(string projectPath, ExecutedStep step)
    {
        var history = await LoadHistoryAsync(projectPath);
        var currentMigration = history.Migrations.LastOrDefault();
        
        if (currentMigration != null)
        {
            currentMigration.ExecutedSteps.Add(step);
            await SaveHistoryAsync(projectPath, history);
        }
    }

    public async Task RecordMigrationCompleteAsync(string projectPath, bool success, string? errorMessage = null)
    {
        var history = await LoadHistoryAsync(projectPath);
        var currentMigration = history.Migrations.LastOrDefault();
        
        if (currentMigration != null)
        {
            currentMigration.Status = success ? "completed" : "failed";
            currentMigration.ErrorMessage = errorMessage;
            currentMigration.Duration = DateTime.UtcNow - currentMigration.ExecutedAt;
            currentMigration.AfterSnapshot = await CaptureSnapshotAsync(projectPath);
            
            if (success)
            {
                history.CurrentLevel = currentMigration.ToLevel;
                history.CurrentSnapshot = currentMigration.AfterSnapshot;
            }
            
            await SaveHistoryAsync(projectPath, history);
        }
    }

    private async Task<ProjectInfo> AnalyzeProjectAsync(string projectDir, string csprojPath)
    {
        var projectName = Path.GetFileNameWithoutExtension(csprojPath);
        var files = Directory.GetFiles(projectDir, "*.cs", SearchOption.AllDirectories);
        
        var projectInfo = new ProjectInfo
        {
            Name = projectName,
            Path = Path.GetRelativePath(Directory.GetCurrentDirectory(), projectDir),
            Type = DetermineProjectType(projectName),
            FileCount = files.Length,
            SizeBytes = files.Sum(f => new FileInfo(f).Length)
        };

        // Analyze references and packages
        if (File.Exists(csprojPath))
        {
            var csprojContent = await File.ReadAllTextAsync(csprojPath);
            projectInfo.References = ExtractProjectReferences(csprojContent);
            projectInfo.Packages = ExtractPackageReferences(csprojContent);
        }

        return projectInfo;
    }

    private string DetermineArchitectureLevel(List<ProjectInfo> projects)
    {
        if (projects.Count == 1) return "minimal";
        if (projects.Count <= 3) return "standard";
        return "enterprise";
    }

    private string DetermineProjectType(string projectName)
    {
        if (projectName.Contains(".Domain")) return "Domain";
        if (projectName.Contains(".Application")) return "Application";
        if (projectName.Contains(".Infrastructure")) return "Infrastructure";
        if (projectName.Contains(".Api")) return "Api";
        return "Unknown";
    }

    private async Task ScanDomainElementsAsync(string projectPath, ProjectSnapshot snapshot)
    {
        var csFiles = Directory.GetFiles(projectPath, "*.cs", SearchOption.AllDirectories);
        
        foreach (var file in csFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            
            // Simple pattern matching - in real implementation would use Roslyn
            if (content.Contains(": AggregateRoot") || content.Contains("class") && content.Contains("Aggregate"))
            {
                var className = ExtractClassName(content);
                if (!string.IsNullOrEmpty(className))
                    snapshot.Aggregates.Add(className);
            }
            
            if (content.Contains("Command") && content.Contains("class"))
            {
                var className = ExtractClassName(content);
                if (!string.IsNullOrEmpty(className))
                    snapshot.Commands.Add(className);
            }
            
            if (content.Contains("Query") && content.Contains("class"))
            {
                var className = ExtractClassName(content);
                if (!string.IsNullOrEmpty(className))
                    snapshot.Queries.Add(className);
            }
        }
    }

    private string ExtractClassName(string content)
    {
        // Simplified class name extraction
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            if (line.Trim().StartsWith("public class"))
            {
                var parts = line.Split(' ');
                var classIndex = Array.IndexOf(parts, "class");
                if (classIndex >= 0 && classIndex + 1 < parts.Length)
                {
                    return parts[classIndex + 1].Split(':')[0].Trim();
                }
            }
        }
        return string.Empty;
    }

    private List<string> ExtractProjectReferences(string csprojContent)
    {
        // Simplified XML parsing - in real implementation would use XDocument
        var references = new List<string>();
        var lines = csprojContent.Split('\n');
        
        foreach (var line in lines)
        {
            if (line.Contains("<ProjectReference") && line.Contains("Include="))
            {
                var start = line.IndexOf("Include=\"") + 9;
                var end = line.IndexOf("\"", start);
                if (start > 8 && end > start)
                {
                    references.Add(line.Substring(start, end - start));
                }
            }
        }
        
        return references;
    }

    private List<string> ExtractPackageReferences(string csprojContent)
    {
        var packages = new List<string>();
        var lines = csprojContent.Split('\n');
        
        foreach (var line in lines)
        {
            if (line.Contains("<PackageReference") && line.Contains("Include="))
            {
                var start = line.IndexOf("Include=\"") + 9;
                var end = line.IndexOf("\"", start);
                if (start > 8 && end > start)
                {
                    var packageName = line.Substring(start, end - start);
                    
                    // Extract version if present
                    var versionStart = line.IndexOf("Version=\"");
                    if (versionStart > 0)
                    {
                        versionStart += 9;
                        var versionEnd = line.IndexOf("\"", versionStart);
                        if (versionEnd > versionStart)
                        {
                            var version = line.Substring(versionStart, versionEnd - versionStart);
                            packageName += $"@{version}";
                        }
                    }
                    
                    packages.Add(packageName);
                }
            }
        }
        
        return packages;
    }

    private async Task<string> GetGitCommitAsync(string projectPath)
    {
        try
        {
            var gitDir = Path.Combine(projectPath, ".git");
            if (Directory.Exists(gitDir))
            {
                var headFile = Path.Combine(gitDir, "HEAD");
                if (File.Exists(headFile))
                {
                    var headContent = await File.ReadAllTextAsync(headFile);
                    if (headContent.StartsWith("ref: "))
                    {
                        var refPath = headContent.Substring(5).Trim();
                        var refFile = Path.Combine(gitDir, refPath);
                        if (File.Exists(refFile))
                        {
                            return (await File.ReadAllTextAsync(refFile)).Trim();
                        }
                    }
                    else
                    {
                        return headContent.Trim();
                    }
                }
            }
        }
        catch
        {
            // Ignore git errors
        }
        
        return string.Empty;
    }
} 