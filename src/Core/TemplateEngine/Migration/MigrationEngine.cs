using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Core.TemplateEngine.Migration;

public interface IMigrationEngine
{
    Task<MigrationPlan> AnalyzeMigrationAsync(string projectPath, TemplateConfiguration targetConfig);
    Task<MigrationResult> ExecuteMigrationAsync(MigrationPlan plan);
    Task<bool> CanMigrateAsync(string projectPath, ArchitectureLevel targetLevel);
}

public class MigrationEngine : IMigrationEngine
{
    private readonly IProjectAnalyzer _projectAnalyzer;
    private readonly ICodeMover _codeMover;
    private readonly ITemplateGenerator _templateGenerator;

    public MigrationEngine(
        IProjectAnalyzer projectAnalyzer,
        ICodeMover codeMover,
        ITemplateGenerator templateGenerator)
    {
        _projectAnalyzer = projectAnalyzer;
        _codeMover = codeMover;
        _templateGenerator = templateGenerator;
    }

    public async Task<MigrationPlan> AnalyzeMigrationAsync(string projectPath, TemplateConfiguration targetConfig)
    {
        var currentStructure = await _projectAnalyzer.AnalyzeAsync(projectPath);
        var targetDecisions = ArchitectureRules.MakeDecisions(targetConfig);
        
        var plan = new MigrationPlan
        {
            SourceStructure = currentStructure,
            TargetStructure = targetDecisions,
            ProjectPath = projectPath
        };

        // Determine migration steps
        plan.Steps = DetermineMigrationSteps(currentStructure, targetDecisions);
        
        return plan;
    }

    public async Task<MigrationResult> ExecuteMigrationAsync(MigrationPlan plan)
    {
        var result = new MigrationResult { Success = true };
        
        try
        {
            foreach (var step in plan.Steps)
            {
                await ExecuteStepAsync(step, plan);
                result.CompletedSteps.Add(step);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Error = ex.Message;
            
            // Rollback completed steps
            await RollbackAsync(result.CompletedSteps, plan);
        }
        
        return result;
    }

    private List<MigrationStep> DetermineMigrationSteps(ProjectStructure current, ArchitectureDecisions target)
    {
        var steps = new List<MigrationStep>();

        // MINIMAL → STANDARD
        if (current.Level == ArchitectureLevel.Minimal && target.ArchitectureLevel >= ArchitectureLevel.Standard)
        {
            steps.AddRange(GetMinimalToStandardSteps());
        }

        // STANDARD → ENTERPRISE  
        if (current.Level <= ArchitectureLevel.Standard && target.ArchitectureLevel == ArchitectureLevel.Enterprise)
        {
            steps.AddRange(GetStandardToEnterpriseSteps(target));
        }

        // Add Infrastructure if needed
        if (!current.HasInfrastructure && target.EnableInfrastructure)
        {
            steps.AddRange(GetInfrastructureSteps(target));
        }

        return steps;
    }

    private List<MigrationStep> GetMinimalToStandardSteps()
    {
        return new List<MigrationStep>
        {
            new CreateProjectStep("Domain", "src/{ServiceName}.Domain"),
            new CreateProjectStep("Application", "src/{ServiceName}.Application"), 
            new CreateProjectStep("Api", "src/{ServiceName}.Api"),
            
            new MoveCodeStep("Domain/*.cs", "src/{ServiceName}/Domain/", "src/{ServiceName}.Domain/"),
            new MoveCodeStep("Application/*.cs", "src/{ServiceName}/Application/", "src/{ServiceName}.Application/"),
            new MoveCodeStep("Controllers/*.cs", "src/{ServiceName}/Controllers/", "src/{ServiceName}.Api/Controllers/"),
            new MoveCodeStep("Program.cs", "src/{ServiceName}/", "src/{ServiceName}.Api/"),
            
            new UpdateProjectReferencesStep(),
            new UpdateNamespacesStep(),
            new UpdateSolutionFileStep(),
            
            new DeleteOldProjectStep("src/{ServiceName}")
        };
    }

    private List<MigrationStep> GetStandardToEnterpriseSteps(ArchitectureDecisions target)
    {
        var steps = new List<MigrationStep>();

        if (target.EnableInfrastructure)
        {
            steps.Add(new CreateProjectStep("Infrastructure", "src/{ServiceName}.Infrastructure"));
            steps.Add(new MoveCodeStep("*Repository.cs", "src/{ServiceName}.Application/", "src/{ServiceName}.Infrastructure/Persistence/"));
        }

        return steps;
    }

    private List<MigrationStep> GetInfrastructureSteps(ArchitectureDecisions target)
    {
        var steps = new List<MigrationStep>
        {
            new CreateProjectStep("Infrastructure", "src/{ServiceName}.Infrastructure")
        };

        // Move persistence code
        steps.Add(new MoveCodeStep("*Repository.cs", "src/{ServiceName}.Application/", "src/{ServiceName}.Infrastructure/Persistence/"));
        steps.Add(new MoveCodeStep("*DbContext.cs", "src/{ServiceName}.Application/", "src/{ServiceName}.Infrastructure/Persistence/"));

        // Generate external services if needed
        if (target.PersistenceStrategy.SeparateReadModel)
        {
            steps.Add(new GenerateCodeStep("ReadModelRepository", "src/{ServiceName}.Infrastructure/Persistence/Read/"));
        }

        return steps;
    }

    private async Task ExecuteStepAsync(MigrationStep step, MigrationPlan plan)
    {
        switch (step)
        {
            case CreateProjectStep createStep:
                await CreateProjectAsync(createStep, plan);
                break;
            case MoveCodeStep moveStep:
                await MoveCodeAsync(moveStep, plan);
                break;
            case UpdateProjectReferencesStep refStep:
                await UpdateReferencesAsync(refStep, plan);
                break;
            case GenerateCodeStep genStep:
                await GenerateCodeAsync(genStep, plan);
                break;
            // ... other step types
        }
    }

    private async Task CreateProjectAsync(CreateProjectStep step, MigrationPlan plan)
    {
        var projectPath = step.Path.Replace("{ServiceName}", plan.SourceStructure.ServiceName);
        var fullPath = Path.Combine(plan.ProjectPath, projectPath);
        
        Directory.CreateDirectory(fullPath);
        
        // Generate .csproj file
        var csprojContent = GenerateCsprojContent(step.ProjectType, plan);
        await File.WriteAllTextAsync(Path.Combine(fullPath, $"{plan.SourceStructure.ServiceName}.{step.ProjectType}.csproj"), csprojContent);
    }

    private async Task MoveCodeAsync(MoveCodeStep step, MigrationPlan plan)
    {
        var sourcePath = step.SourcePath.Replace("{ServiceName}", plan.SourceStructure.ServiceName);
        var targetPath = step.TargetPath.Replace("{ServiceName}", plan.SourceStructure.ServiceName);
        
        await _codeMover.MoveAsync(
            Path.Combine(plan.ProjectPath, sourcePath),
            Path.Combine(plan.ProjectPath, targetPath),
            step.Pattern);
    }

    public async Task<bool> CanMigrateAsync(string projectPath, ArchitectureLevel targetLevel)
    {
        var structure = await _projectAnalyzer.AnalyzeAsync(projectPath);
        
        // Can always migrate up, but not down (for safety)
        return structure.Level <= targetLevel;
    }

    private async Task RollbackAsync(List<MigrationStep> completedSteps, MigrationPlan plan)
    {
        // Implement rollback logic
        foreach (var step in completedSteps.AsEnumerable().Reverse())
        {
            await RollbackStepAsync(step, plan);
        }
    }

    private async Task UpdateReferencesAsync(UpdateProjectReferencesStep step, MigrationPlan plan)
    {
        await Task.Delay(100); // Simulate updating project references
        // Implementation would update .csproj files with new project references
    }

    private async Task GenerateCodeAsync(GenerateCodeStep step, MigrationPlan plan)
    {
        await _templateGenerator.GenerateAsync(step.CodeType, step.TargetPath, new Dictionary<string, object>
        {
            ["ServiceName"] = plan.SourceStructure.ServiceName
        });
    }

    private Task RollbackStepAsync(MigrationStep step, MigrationPlan plan)
    {
        // Implement step-specific rollback
        switch (step)
        {
            case CreateProjectStep createStep:
                var projectPath = createStep.Path.Replace("{ServiceName}", plan.SourceStructure.ServiceName);
                var fullPath = Path.Combine(plan.ProjectPath, projectPath);
                if (Directory.Exists(fullPath))
                    Directory.Delete(fullPath, true);
                break;
            // ... other rollback implementations
        }
        
        return Task.CompletedTask;
    }

    private string GenerateCsprojContent(string projectType, MigrationPlan plan)
    {
        return projectType switch
        {
            "Domain" => """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                  <ItemGroup>
                    <PackageReference Include="AggregateKit" Version="0.2.0" />
                  </ItemGroup>
                </Project>
                """,
            "Application" => """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>
                  <ItemGroup>
                    <ProjectReference Include="..\{ServiceName}.Domain\{ServiceName}.Domain.csproj" />
                  </ItemGroup>
                  <ItemGroup>
                    <PackageReference Include="WolverineFx" Version="3.5.0" />
                    <PackageReference Include="FluentValidation" Version="11.9.0" />
                  </ItemGroup>
                </Project>
                """.Replace("{ServiceName}", plan.SourceStructure.ServiceName),
            _ => throw new ArgumentException($"Unknown project type: {projectType}")
        };
    }
} 