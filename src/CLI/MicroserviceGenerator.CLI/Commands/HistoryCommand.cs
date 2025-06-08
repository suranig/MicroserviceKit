using System.CommandLine;
using System.Text.Json;
using Microservice.Core.TemplateEngine.Migration;

namespace MicroserviceGenerator.CLI.Commands;

public static class HistoryCommand
{
    public static Command Create()
    {
        var command = new Command("history", "Show migration history and project evolution");

        var pathOption = new Option<string>(
            "--path",
            description: "Path to microservice project",
            getDefaultValue: () => Directory.GetCurrentDirectory());

        var formatOption = new Option<string>(
            "--format",
            description: "Output format (table, json, summary)",
            getDefaultValue: () => "table");

        var showSnapshotsOption = new Option<bool>(
            "--snapshots",
            "Show detailed snapshots");

        command.AddOption(pathOption);
        command.AddOption(formatOption);
        command.AddOption(showSnapshotsOption);

        command.SetHandler(async (path, format, showSnapshots) =>
        {
            await ExecuteHistoryAsync(path, format, showSnapshots);
        }, pathOption, formatOption, showSnapshotsOption);

        return command;
    }

    private static async Task ExecuteHistoryAsync(string path, string format, bool showSnapshots)
    {
        try
        {
            var historyService = new MigrationHistoryService();
            var history = await historyService.LoadHistoryAsync(path);

            switch (format.ToLowerInvariant())
            {
                case "json":
                    await ShowJsonHistoryAsync(history);
                    break;
                case "summary":
                    ShowSummaryHistory(history);
                    break;
                default:
                    ShowTableHistory(history, showSnapshots);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading migration history: {ex.Message}");
        }
    }

    private static async Task ShowJsonHistoryAsync(MigrationHistory history)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(history, options);
        Console.WriteLine(json);
    }

    private static void ShowSummaryHistory(MigrationHistory history)
    {
        Console.WriteLine($"📊 Migration History Summary for {history.ServiceName}");
        Console.WriteLine();
        Console.WriteLine($"🏗️  Current Architecture: {history.CurrentLevel.ToUpperInvariant()}");
        Console.WriteLine($"📅 Created: {history.CreatedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"🔄 Last Migration: {history.LastMigrationAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"📈 Total Migrations: {history.Migrations.Count}");
        Console.WriteLine();

        if (history.CurrentSnapshot != null)
        {
            Console.WriteLine("📦 Current State:");
            Console.WriteLine($"   Projects: {history.CurrentSnapshot.Projects.Count}");
            Console.WriteLine($"   Aggregates: {history.CurrentSnapshot.Aggregates.Count}");
            Console.WriteLine($"   Commands: {history.CurrentSnapshot.Commands.Count}");
            Console.WriteLine($"   Queries: {history.CurrentSnapshot.Queries.Count}");
            Console.WriteLine($"   External Services: {history.CurrentSnapshot.ExternalServices.Count}");
        }

        if (history.Metadata.ContainsKey("teamSize"))
        {
            Console.WriteLine();
            Console.WriteLine("👥 Team Information:");
            Console.WriteLine($"   Team Size: {history.Metadata["teamSize"]}");
            if (history.Metadata.ContainsKey("projectComplexity"))
                Console.WriteLine($"   Complexity: {history.Metadata["projectComplexity"]}");
        }
    }

    private static void ShowTableHistory(MigrationHistory history, bool showSnapshots)
    {
        Console.WriteLine($"📋 Migration History for {history.ServiceName}");
        Console.WriteLine($"🏗️  Current Level: {history.CurrentLevel.ToUpperInvariant()}");
        Console.WriteLine();

        if (!history.Migrations.Any())
        {
            Console.WriteLine("No migrations found. This project was created at current level.");
            return;
        }

        Console.WriteLine("🔄 Migration Timeline:");
        Console.WriteLine();

        foreach (var migration in history.Migrations.OrderBy(m => m.ExecutedAt))
        {
            var statusIcon = migration.Status switch
            {
                "completed" => "✅",
                "failed" => "❌",
                "rolled_back" => "🔄",
                _ => "⏳"
            };

            Console.WriteLine($"{statusIcon} Migration {migration.Id}");
            Console.WriteLine($"   📅 Date: {migration.ExecutedAt:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"   🔀 Path: {migration.FromLevel} → {migration.ToLevel}");
            Console.WriteLine($"   ⏱️  Duration: {migration.Duration.TotalMinutes:F1} minutes");
            Console.WriteLine($"   📝 Steps: {migration.ExecutedSteps.Count}");

            if (migration.Status == "failed" && !string.IsNullOrEmpty(migration.ErrorMessage))
            {
                Console.WriteLine($"   ❌ Error: {migration.ErrorMessage}");
            }

            if (showSnapshots)
            {
                ShowSnapshotComparison(migration);
            }

            Console.WriteLine();
        }

        if (history.CurrentSnapshot != null)
        {
            Console.WriteLine("📊 Current Project State:");
            ShowProjectSnapshot(history.CurrentSnapshot);
        }
    }

    private static void ShowSnapshotComparison(MigrationRecord migration)
    {
        Console.WriteLine("   📊 Changes:");
        
        var before = migration.BeforeSnapshot;
        var after = migration.AfterSnapshot;

        Console.WriteLine($"      Projects: {before.Projects.Count} → {after.Projects.Count}");
        Console.WriteLine($"      Aggregates: {before.Aggregates.Count} → {after.Aggregates.Count}");
        Console.WriteLine($"      Commands: {before.Commands.Count} → {after.Commands.Count}");
        Console.WriteLine($"      Queries: {before.Queries.Count} → {after.Queries.Count}");

        // Show new projects added
        var newProjects = after.Projects.Where(p => !before.Projects.Any(bp => bp.Name == p.Name)).ToList();
        if (newProjects.Any())
        {
            Console.WriteLine($"      ➕ New Projects: {string.Join(", ", newProjects.Select(p => p.Name))}");
        }

        // Show new aggregates
        var newAggregates = after.Aggregates.Except(before.Aggregates).ToList();
        if (newAggregates.Any())
        {
            Console.WriteLine($"      ➕ New Aggregates: {string.Join(", ", newAggregates)}");
        }

        // Show files created/modified
        var totalFilesCreated = migration.ExecutedSteps.SelectMany(s => s.FilesCreated).Count();
        var totalFilesModified = migration.ExecutedSteps.SelectMany(s => s.FilesModified).Count();
        var totalFilesDeleted = migration.ExecutedSteps.SelectMany(s => s.FilesDeleted).Count();

        if (totalFilesCreated > 0 || totalFilesModified > 0 || totalFilesDeleted > 0)
        {
            Console.WriteLine($"      📁 Files: +{totalFilesCreated} ~{totalFilesModified} -{totalFilesDeleted}");
        }
    }

    private static void ShowProjectSnapshot(ProjectSnapshot snapshot)
    {
        Console.WriteLine($"   📅 Captured: {snapshot.CapturedAt:yyyy-MM-dd HH:mm}");
        Console.WriteLine($"   🏗️  Level: {snapshot.ArchitectureLevel}");
        
        if (!string.IsNullOrEmpty(snapshot.GitCommit))
        {
            Console.WriteLine($"   🔗 Git: {snapshot.GitCommit[..Math.Min(8, snapshot.GitCommit.Length)]}");
        }

        Console.WriteLine();
        Console.WriteLine("   📦 Projects:");
        foreach (var project in snapshot.Projects.OrderBy(p => p.Type))
        {
            var sizeKb = project.SizeBytes / 1024.0;
            Console.WriteLine($"      {GetProjectTypeIcon(project.Type)} {project.Name}");
            Console.WriteLine($"         📁 {project.FileCount} files ({sizeKb:F1} KB)");
            Console.WriteLine($"         📦 {project.Packages.Count} packages");
        }

        if (snapshot.Aggregates.Any())
        {
            Console.WriteLine();
            Console.WriteLine($"   🏛️  Aggregates: {string.Join(", ", snapshot.Aggregates)}");
        }

        if (snapshot.ExternalServices.Any())
        {
            Console.WriteLine($"   🌐 External Services: {string.Join(", ", snapshot.ExternalServices)}");
        }

        if (snapshot.CustomFiles.Any())
        {
            Console.WriteLine();
            Console.WriteLine("   ⚠️  Custom Files (preserved during migration):");
            foreach (var file in snapshot.CustomFiles.Take(5))
            {
                Console.WriteLine($"      📄 {file}");
            }
            if (snapshot.CustomFiles.Count > 5)
            {
                Console.WriteLine($"      ... and {snapshot.CustomFiles.Count - 5} more");
            }
        }
    }

    private static string GetProjectTypeIcon(string projectType)
    {
        return projectType switch
        {
            "Domain" => "🏛️",
            "Application" => "⚙️",
            "Api" => "🌐",
            "Infrastructure" => "🔧",
            _ => "📦"
        };
    }
} 