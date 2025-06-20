using FluentAssertions;
using MicroserviceGenerator.CLI.Services;
using Xunit;

namespace CLI.Tests.Unit.Services;

public class TemplateServiceTests
{
    private readonly TemplateService _templateService;

    public TemplateServiceTests()
    {
        // Change working directory to project root for tests
        var projectRoot = Path.GetFullPath("../../");
        Directory.SetCurrentDirectory(projectRoot);
        _templateService = new TemplateService();
    }

    [Fact]
    public async Task LoadTemplatesAsync_ShouldReturnTemplates_WhenIndexExists()
    {
        // Act
        var templates = await _templateService.LoadTemplatesAsync();

        // Assert
        templates.Should().NotBeEmpty();
        templates.Should().ContainSingle(t => t.Name == "minimal-service.json");
        templates.Should().ContainSingle(t => t.Name == "standard-service.json");
        templates.Should().ContainSingle(t => t.Name == "enterprise-service.json");
    }

    [Fact]
    public async Task LoadTemplateAsync_ShouldReturnTemplate_WhenTemplateExists()
    {
        // Act
        var template = await _templateService.LoadTemplateAsync("minimal-service.json");

        // Assert
        template.Should().NotBeNull();
        template!.Name.Should().Be("minimal-service.json");
        template.Title.Should().Be("Minimal Service");
        template.Category.Should().Be("levels");
        template.Complexity.Should().Be("simple");
    }

    [Fact]
    public async Task LoadTemplateAsync_ShouldReturnNull_WhenTemplateDoesNotExist()
    {
        // Act
        var template = await _templateService.LoadTemplateAsync("non-existent-template.json");

        // Assert
        template.Should().BeNull();
    }

    [Fact]
    public async Task LoadTemplateConfigurationAsync_ShouldReturnConfiguration_WhenTemplateExists()
    {
        // Act
        var config = await _templateService.LoadTemplateConfigurationAsync("minimal-service.json");

        // Assert
        config.Should().NotBeNull();
        config!.MicroserviceName.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoadTemplateConfigurationAsync_ShouldReturnNull_WhenTemplateDoesNotExist()
    {
        // Act
        var config = await _templateService.LoadTemplateConfigurationAsync("non-existent-template.json");

        // Assert
        config.Should().BeNull();
    }

    [Theory]
    [InlineData("levels")]
    [InlineData("examples")]
    [InlineData("configs")]
    public async Task LoadTemplatesAsync_ShouldReturnTemplatesFromAllCategories(string category)
    {
        // Act
        var templates = await _templateService.LoadTemplatesAsync();

        // Assert
        templates.Should().Contain(t => t.Category == category);
    }
} 