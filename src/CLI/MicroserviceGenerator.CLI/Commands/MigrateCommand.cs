using System.CommandLine;
using Microservice.Core.TemplateEngine.Configuration;
using Microservice.Core.TemplateEngine.Migration;

namespace MicroserviceGenerator.CLI.Commands;

public static class MigrateCommand
{
    public static Command Create()
    {
        var command = new Command("migrate", "Migrate existing microservice to different architecture level");

        var pathOption = new Option<string>(
            "--path",
            description: "Path to existing microservice project",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        var configOption = new Option<FileInfo?>(
            "--config",
            "Configuration file for target architecture");

        var levelOption = new Option<string?>(
            "--level",
            "Target architecture level (minimal, standard, enterprise)");

        var dryRunOption = new Option<bool>(
            "--dry-run",
            "Show migration plan without executing");

        var forceOption = new Option<bool>(
            "--force",
            "Force migration even if risky");

        command.AddOption(pathOption);
        command.AddOption(configOption);
        command.AddOption(levelOption);
        command.AddOption(dryRunOption);
        command.AddOption(forceOption);

        command.SetHandler(async (path, configFile, level, dryRun, force) =>
        {
            await ExecuteMigrateAsync(path, configFile, level, dryRun, force);
        }, pathOption, configOption, levelOption, dryRunOption, forceOption);

        return command;
    }

    private static async Task ExecuteMigrateAsync(
        string path,
        FileInfo? configFile,
        string? level,
        bool dryRun,
        bool force)
    {
        try
        {
            Console.WriteLine($"🔄 Analyzing project at: {path}");

            // Load target configuration
            var targetConfig = await LoadTargetConfigurationAsync(configFile, level);
            if (targetConfig == null)
            {
                Console.WriteLine("❌ Could not determine target configuration");
                return;
            }

            // Create migration engine (simplified for demo)
            var migrationEngine = CreateMigrationEngine();

            // Check if migration is possible
            var levelString = targetConfig.Architecture?.Level ?? "standard";
            var targetLevel = levelString.ToLowerInvariant() switch
            {
                "minimal" => ArchitectureLevel.Minimal,
                "enterprise" => ArchitectureLevel.Enterprise,
                _ => ArchitectureLevel.Standard
            };
            var canMigrate = await migrationEngine.CanMigrateAsync(path, targetLevel);
            if (!canMigrate && !force)
            {
                Console.WriteLine("❌ Migration not possible. Use --force to override.");
                return;
            }

            // Analyze migration
            Console.WriteLine("📊 Analyzing migration requirements...");
            var plan = await migrationEngine.AnalyzeMigrationAsync(path, targetConfig);

            // Show migration plan
            ShowMigrationPlan(plan);

            if (dryRun)
            {
                Console.WriteLine("🔍 Dry run completed. Use without --dry-run to execute migration.");
                return;
            }

            // Confirm migration
            if (!force && !ConfirmMigration(plan))
            {
                Console.WriteLine("❌ Migration cancelled by user.");
                return;
            }

            // Execute migration
            Console.WriteLine("🚀 Executing migration...");
            var result = await migrationEngine.ExecuteMigrationAsync(plan);

            if (result.Success)
            {
                Console.WriteLine("✅ Migration completed successfully!");
                ShowMigrationResult(result);
            }
            else
            {
                Console.WriteLine($"❌ Migration failed: {result.Error}");
                Console.WriteLine("🔄 Rollback completed automatically.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during migration: {ex.Message}");
        }
    }

    private static async Task<TemplateConfiguration?> LoadTargetConfigurationAsync(FileInfo? configFile, string? level)
    {
        if (configFile != null && configFile.Exists)
        {
            var json = await File.ReadAllTextAsync(configFile.FullName);
            return System.Text.Json.JsonSerializer.Deserialize<TemplateConfiguration>(json);
        }

        if (!string.IsNullOrEmpty(level))
        {
            return CreateConfigurationForLevel(level);
        }

        // Interactive mode
        return await CreateInteractiveConfigurationAsync();
    }

    private static TemplateConfiguration CreateConfigurationForLevel(string level)
    {
        var architectureLevel = level.ToLowerInvariant() switch
        {
            "minimal" => ArchitectureLevel.Minimal,
            "standard" => ArchitectureLevel.Standard,
            "enterprise" => ArchitectureLevel.Enterprise,
            _ => throw new ArgumentException($"Unknown architecture level: {level}")
        };

        return new TemplateConfiguration
        {
            MicroserviceName = "ExistingService", // Will be detected from project
            Architecture = new ArchitectureConfiguration { Level = architectureLevel.ToString().ToLowerInvariant() },
            Domain = new DomainConfiguration
            {
                Aggregates = new List<AggregateConfiguration>() // Will be detected
            }
        };
    }

    private static async Task<TemplateConfiguration> CreateInteractiveConfigurationAsync()
    {
        Console.WriteLine("🎯 Interactive Migration Configuration");
        Console.WriteLine();

        Console.Write("Target architecture level (minimal/standard/enterprise): ");
        var levelInput = Console.ReadLine()?.ToLowerInvariant() ?? "standard";

        var level = levelInput switch
        {
            "minimal" => ArchitectureLevel.Minimal,
            "enterprise" => ArchitectureLevel.Enterprise,
            _ => ArchitectureLevel.Standard
        };

        return new TemplateConfiguration
        {
            MicroserviceName = "ExistingService",
            Architecture = new ArchitectureConfiguration { Level = level.ToString().ToLowerInvariant() },
            Domain = new DomainConfiguration
            {
                Aggregates = new List<AggregateConfiguration>()
            }
        };
    }

    private static IMigrationEngine CreateMigrationEngine()
    {
        // Simplified factory - in real implementation would use DI
        var projectAnalyzer = new ProjectAnalyzer();
        var codeMover = new CodeMover();
        var templateGenerator = new TemplateGenerator();

        return new MigrationEngine(projectAnalyzer, codeMover, templateGenerator);
    }

    private static void ShowMigrationPlan(MigrationPlan plan)
    {
        Console.WriteLine();
        Console.WriteLine("📋 Migration Plan:");
        Console.WriteLine($"   From: {plan.SourceStructure.Level} ({plan.SourceStructure.Projects.Count} projects)");
        Console.WriteLine($"   To:   {plan.TargetStructure.ArchitectureLevel} ({EstimateTargetProjects(plan.TargetStructure)} projects)");
        Console.WriteLine($"   Duration: ~{plan.EstimatedDuration.TotalMinutes:F0} minutes");
        Console.WriteLine();

        Console.WriteLine("📝 Steps:");
        for (int i = 0; i < plan.Steps.Count; i++)
        {
            var step = plan.Steps[i];
            var icon = step.IsReversible ? "🔄" : "⚠️";
            Console.WriteLine($"   {i + 1:D2}. {icon} {step}");
        }

        if (plan.Warnings.Any())
        {
            Console.WriteLine();
            Console.WriteLine("⚠️  Warnings:");
            foreach (var warning in plan.Warnings)
            {
                Console.WriteLine($"   • {warning}");
            }
        }
    }

    private static bool ConfirmMigration(MigrationPlan plan)
    {
        Console.WriteLine();
        Console.Write("Do you want to proceed with this migration? (y/N): ");
        var response = Console.ReadLine()?.ToLowerInvariant();
        return response == "y" || response == "yes";
    }

    private static void ShowMigrationResult(MigrationResult result)
    {
        Console.WriteLine();
        Console.WriteLine($"⏱️  Duration: {result.Duration.TotalSeconds:F1} seconds");
        Console.WriteLine($"📁 Generated files: {result.GeneratedFiles.Count}");
        Console.WriteLine($"📝 Modified files: {result.ModifiedFiles.Count}");

        if (result.GeneratedFiles.Any())
        {
            Console.WriteLine();
            Console.WriteLine("📁 Generated files:");
            foreach (var file in result.GeneratedFiles.Take(10))
            {
                Console.WriteLine($"   + {file}");
            }
            if (result.GeneratedFiles.Count > 10)
            {
                Console.WriteLine($"   ... and {result.GeneratedFiles.Count - 10} more");
            }
        }
    }

    private static int EstimateTargetProjects(ArchitectureDecisions decisions)
    {
        var count = 1; // Always have at least one project

        if (decisions.ArchitectureLevel >= ArchitectureLevel.Standard)
        {
            count = 3; // Domain, Application, Api
        }

        if (decisions.EnableInfrastructure)
        {
            count++;
        }

        return count;
    }
}

// Simplified implementations for demo
internal class ProjectAnalyzer : IProjectAnalyzer
{
    public async Task<Microservice.Core.TemplateEngine.Migration.ProjectStructure> AnalyzeAsync(string projectPath)
    {
        // Simplified analysis - in real implementation would scan files
        await Task.Delay(100); // Simulate analysis time

        return new Microservice.Core.TemplateEngine.Migration.ProjectStructure
        {
            ServiceName = Path.GetFileName(projectPath),
            Level = ArchitectureLevel.Minimal, // Detected from structure
            HasInfrastructure = false,
            HasDomainLayer = false,
            HasApplicationLayer = false,
            Projects = new List<string> { "SingleProject" },
            Aggregates = new List<string> { "Todo" },
            Commands = new List<string> { "CreateTodo" },
            Queries = new List<string> { "GetTodos" }
        };
    }
}

internal class CodeMover : ICodeMover
{
    public async Task MoveAsync(string sourcePath, string targetPath, string pattern)
    {
        await Task.Delay(50); // Simulate file operations
        // Implementation would move files matching pattern
    }

    public async Task<bool> CanMoveAsync(string sourcePath, string targetPath)
    {
        await Task.Delay(10);
        return true; // Simplified check
    }
}

internal class TemplateGenerator : ITemplateGenerator
{
    public async Task GenerateAsync(string templateName, string targetPath, Dictionary<string, object> parameters)
    {
        await Task.Delay(100); // Simulate code generation
        // Implementation would generate code from templates
    }
} 