using FluentAssertions;
using System.Diagnostics;
using Xunit;

namespace CLI.Tests.Integration;

public class CLIIntegrationTests : IDisposable
{
    private readonly string _testOutputDirectory;
    private readonly string _cliPath;

    public CLIIntegrationTests()
    {
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), $"cli_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testOutputDirectory);
        
        // Path to CLI project - adjust based on test execution context
        _cliPath = Path.GetFullPath("../../src/CLI/MicroserviceGenerator.CLI");
    }

    [Fact]
    public async Task CLI_List_ShouldReturnTemplates()
    {
        // Act
        var result = await RunCLICommandAsync("list");

        // Assert
        result.ExitCode.Should().Be(0);
        result.Output.Should().Contain("Available Templates");
        result.Output.Should().Contain("minimal-service.json");
        result.Output.Should().Contain("standard-service.json");
        result.Output.Should().Contain("enterprise-service.json");
    }

    [Fact]
    public async Task CLI_Describe_ShouldReturnTemplateDetails()
    {
        // Act
        var result = await RunCLICommandAsync("describe minimal-service.json");

        // Assert
        result.ExitCode.Should().Be(0);
        result.Output.Should().Contain("Template: Minimal Service");
        result.Output.Should().Contain("Single project, basic CRUD");
        result.Output.Should().Contain("simple");
        result.Output.Should().Contain("5 minutes");
    }

    [Fact]
    public async Task CLI_Generate_ShouldCreateMicroservice()
    {
        // Arrange
        var serviceName = "TestService";
        var outputPath = Path.Combine(_testOutputDirectory, serviceName);

        // Act
        var result = await RunCLICommandAsync($"generate {serviceName} --template minimal-service.json --output {outputPath}");

        // Assert
        result.ExitCode.Should().Be(0);
        result.Output.Should().Contain($"Microservice '{serviceName}' generated successfully");

        // Verify generated files
        Directory.Exists(outputPath).Should().BeTrue();
        File.Exists(Path.Combine(outputPath, $"{serviceName}.sln")).Should().BeTrue();
        File.Exists(Path.Combine(outputPath, "README.md")).Should().BeTrue();
        Directory.Exists(Path.Combine(outputPath, "src")).Should().BeTrue();
        Directory.Exists(Path.Combine(outputPath, "tests")).Should().BeTrue();
    }

    [Fact]
    public async Task CLI_Generate_WithCustomAggregates_ShouldIncludeThemInOutput()
    {
        // Arrange
        var serviceName = "CustomService";
        var outputPath = Path.Combine(_testOutputDirectory, serviceName);

        // Act
        var result = await RunCLICommandAsync($"generate {serviceName} --template minimal-service.json --output {outputPath} --aggregates Order Product");

        // Assert
        result.ExitCode.Should().Be(0);
        
        var readmePath = Path.Combine(outputPath, "README.md");
        File.Exists(readmePath).Should().BeTrue();
        
        var readmeContent = await File.ReadAllTextAsync(readmePath);
        readmeContent.Should().Contain("Order");
        readmeContent.Should().Contain("Product");
    }

    [Fact]
    public async Task CLI_Generate_WithInvalidServiceName_ShouldFail()
    {
        // Act
        var result = await RunCLICommandAsync($"generate \"Invalid Service\" --template minimal-service.json --output {_testOutputDirectory}");

        // Assert
        result.ExitCode.Should().Be(1);
        result.Output.Should().Contain("Validation errors");
        result.Output.Should().Contain("valid C# identifier");
    }

    [Fact]
    public async Task CLI_Generate_WithNonExistentTemplate_ShouldFail()
    {
        // Act
        var result = await RunCLICommandAsync($"generate TestService --template non-existent.json --output {_testOutputDirectory}");

        // Assert
        result.ExitCode.Should().Be(1);
        result.Output.Should().Contain("Template not found");
    }

    [Fact]
    public async Task CLI_Help_ShouldShowUsageInformation()
    {
        // Act
        var result = await RunCLICommandAsync("--help");

        // Assert
        result.ExitCode.Should().Be(0);
        result.Output.Should().Contain("MicroserviceKit");
        result.Output.Should().Contain("generate");
        result.Output.Should().Contain("list");
        result.Output.Should().Contain("describe");
    }

    private async Task<CLIResult> RunCLICommandAsync(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project {_cliPath} -- {arguments}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();

        return new CLIResult
        {
            ExitCode = process.ExitCode,
            Output = output + error
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_testOutputDirectory))
        {
            try
            {
                Directory.Delete(_testOutputDirectory, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private record CLIResult
    {
        public int ExitCode { get; init; }
        public string Output { get; init; } = string.Empty;
    }
} 