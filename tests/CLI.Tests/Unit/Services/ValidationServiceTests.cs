using FluentAssertions;
using MicroserviceGenerator.CLI.Models;
using MicroserviceGenerator.CLI.Services;
using Microservice.Core.TemplateEngine.Configuration;
using Xunit;

namespace CLI.Tests.Unit.Services;

public class ValidationServiceTests
{
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        _validationService = new ValidationService();
    }

    [Fact]
    public void ValidateTemplate_ShouldReturnValid_WhenConfigurationIsValid()
    {
        // Arrange
        var config = new TemplateConfiguration
        {
            MicroserviceName = "TestService",
            Namespace = "Company.TestService"
        };

        // Act
        var result = _validationService.ValidateTemplate(config);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateTemplate_ShouldReturnInvalid_WhenMicroserviceNameIsEmpty()
    {
        // Arrange
        var config = new TemplateConfiguration
        {
            MicroserviceName = "",
            Namespace = "Company.TestService"
        };

        // Act
        var result = _validationService.ValidateTemplate(config);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("MicroserviceName is required");
    }

    [Theory]
    [InlineData("Test Service")]
    [InlineData("Test-Service")]
    [InlineData("123TestService")]
    [InlineData("Test.Service")]
    public void ValidateTemplate_ShouldReturnInvalid_WhenMicroserviceNameIsInvalid(string invalidName)
    {
        // Arrange
        var config = new TemplateConfiguration
        {
            MicroserviceName = invalidName,
            Namespace = "Company.TestService"
        };

        // Act
        var result = _validationService.ValidateTemplate(config);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("valid C# identifier"));
    }

    [Fact]
    public void ValidateGenerationOptions_ShouldReturnValid_WhenOptionsAreValid()
    {
        // Arrange
        var options = new GenerationOptions
        {
            ServiceName = "TestService",
            TemplateName = "minimal-service.json",
            OutputPath = "/tmp/test"
        };

        // Act
        var result = _validationService.ValidateGenerationOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidateGenerationOptions_ShouldReturnInvalid_WhenServiceNameIsEmpty()
    {
        // Arrange
        var options = new GenerationOptions
        {
            ServiceName = "",
            TemplateName = "minimal-service.json",
            OutputPath = "/tmp/test"
        };

        // Act
        var result = _validationService.ValidateGenerationOptions(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Service name is required");
    }

    [Theory]
    [InlineData("postgresql")]
    [InlineData("sqlserver")]
    [InlineData("mysql")]
    [InlineData("sqlite")]
    [InlineData("inmemory")]
    public void ValidateGenerationOptions_ShouldReturnValid_WhenDatabaseProviderIsSupported(string provider)
    {
        // Arrange
        var options = new GenerationOptions
        {
            ServiceName = "TestService",
            TemplateName = "minimal-service.json",
            OutputPath = "/tmp/test",
            DatabaseProvider = provider
        };

        // Act
        var result = _validationService.ValidateGenerationOptions(options);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("oracle")]
    [InlineData("mongodb")]
    [InlineData("invalid")]
    public void ValidateGenerationOptions_ShouldReturnInvalid_WhenDatabaseProviderIsUnsupported(string provider)
    {
        // Arrange
        var options = new GenerationOptions
        {
            ServiceName = "TestService",
            TemplateName = "minimal-service.json",
            OutputPath = "/tmp/test",
            DatabaseProvider = provider
        };

        // Act
        var result = _validationService.ValidateGenerationOptions(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Database provider") && error.Contains("not supported"));
    }

    [Fact]
    public void ValidateGenerationOptions_ShouldReturnInvalid_WhenCustomAggregateNameIsInvalid()
    {
        // Arrange
        var options = new GenerationOptions
        {
            ServiceName = "TestService",
            TemplateName = "minimal-service.json",
            OutputPath = "/tmp/test",
            CustomAggregates = new List<string> { "Invalid Name" }
        };

        // Act
        var result = _validationService.ValidateGenerationOptions(options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Aggregate name") && error.Contains("valid C# class name"));
    }
} 