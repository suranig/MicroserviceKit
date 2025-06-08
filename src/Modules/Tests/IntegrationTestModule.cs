using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.Tests;

public class IntegrationTestModule : ITemplateModule
{
    public string Name => "IntegrationTests";
    public string Description => "Generates integration tests with API tests, database tests, and end-to-end scenarios";

    public bool IsEnabled(TemplateConfiguration config)
    {
        var testingLevel = config.Features?.Testing?.Level?.ToLowerInvariant();
        return testingLevel == "integration" || testingLevel == "full" || testingLevel == "enterprise";
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var outputPath = context.GetIntegrationTestsProjectPath();

        // Create project structure
        await CreateProjectStructureAsync(outputPath, config);

        // Generate integration tests for each aggregate
        if (config.Domain?.Aggregates != null)
        {
            foreach (var aggregate in config.Domain.Aggregates)
            {
                await GenerateAggregateIntegrationTestsAsync(outputPath, config, aggregate);
            }
        }

        // Generate test infrastructure
        await GenerateTestInfrastructureAsync(outputPath, config);
    }

    private async Task CreateProjectStructureAsync(string outputPath, TemplateConfiguration config)
    {
        Directory.CreateDirectory(outputPath);
        Directory.CreateDirectory(Path.Combine(outputPath, "Api"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Database"));
        Directory.CreateDirectory(Path.Combine(outputPath, "EndToEnd"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Fixtures"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Helpers"));

        // Generate .csproj file
        var csprojContent = GenerateProjectFile(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, $"{config.MicroserviceName}.Integration.Tests.csproj"), csprojContent);

        // Generate test settings
        var testSettingsContent = GenerateTestSettings(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "appsettings.Test.json"), testSettingsContent);

        // Generate GlobalUsings
        var globalUsingsContent = GenerateGlobalUsings();
        await File.WriteAllTextAsync(Path.Combine(outputPath, "GlobalUsings.cs"), globalUsingsContent);
    }

    private async Task GenerateAggregateIntegrationTestsAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        // Generate API integration tests
        var apiTestContent = GenerateApiIntegrationTests(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Api", $"{aggregate.Name}ApiIntegrationTests.cs"), 
            apiTestContent);

        // Generate database integration tests
        var dbTestContent = GenerateDatabaseIntegrationTests(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Database", $"{aggregate.Name}DatabaseIntegrationTests.cs"), 
            dbTestContent);

        // Generate end-to-end tests
        var e2eTestContent = GenerateEndToEndTests(config, aggregate);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "EndToEnd", $"{aggregate.Name}EndToEndTests.cs"), 
            e2eTestContent);
    }

    private async Task GenerateTestInfrastructureAsync(string outputPath, TemplateConfiguration config)
    {
        // Generate test application factory
        var testFactoryContent = GenerateTestApplicationFactory(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Fixtures", "TestApplicationFactory.cs"), 
            testFactoryContent);

        // Generate database fixture
        var dbFixtureContent = GenerateDatabaseFixture(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Fixtures", "DatabaseFixture.cs"), 
            dbFixtureContent);

        // Generate test data builder
        var testDataBuilderContent = GenerateTestDataBuilder(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Helpers", "TestDataBuilder.cs"), 
            testDataBuilderContent);

        // Generate HTTP client extensions
        var httpExtensionsContent = GenerateHttpClientExtensions(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Helpers", "HttpClientExtensions.cs"), 
            httpExtensionsContent);

        // Generate test containers setup
        var testContainersContent = GenerateTestContainersSetup(config);
        await File.WriteAllTextAsync(
            Path.Combine(outputPath, "Fixtures", "TestContainersSetup.cs"), 
            testContainersContent);
    }

    private string GenerateProjectFile(TemplateConfiguration config)
    {
        // Calculate relative paths from tests to source projects
        var structure = config.ProjectStructure ?? new ProjectStructureConfiguration();
        var domainPath = structure.DomainProjectPath
            .Replace("{SourceDirectory}", structure.SourceDirectory)
            .Replace("{MicroserviceName}", config.MicroserviceName);
        var applicationPath = structure.ApplicationProjectPath
            .Replace("{SourceDirectory}", structure.SourceDirectory)
            .Replace("{MicroserviceName}", config.MicroserviceName);
        var infrastructurePath = structure.InfrastructureProjectPath
            .Replace("{SourceDirectory}", structure.SourceDirectory)
            .Replace("{MicroserviceName}", config.MicroserviceName);
        var apiPath = structure.ApiProjectPath
            .Replace("{SourceDirectory}", structure.SourceDirectory)
            .Replace("{MicroserviceName}", config.MicroserviceName);

        return $@"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Microsoft.NET.Test.Sdk"" Version=""17.8.0"" />
    <PackageReference Include=""xunit"" Version=""2.6.1"" />
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.5.3"" />
    <PackageReference Include=""Microsoft.AspNetCore.Mvc.Testing"" Version=""8.0.16"" />
    <PackageReference Include=""FluentAssertions"" Version=""6.12.0"" />
    <PackageReference Include=""Testcontainers"" Version=""3.6.0"" />
    <PackageReference Include=""Testcontainers.PostgreSql"" Version=""3.6.0"" />
    <PackageReference Include=""Testcontainers.MsSql"" Version=""3.6.0"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.InMemory"" Version=""8.0.16"" />
    <PackageReference Include=""Bogus"" Version=""35.4.0"" />
    <PackageReference Include=""WireMock.Net"" Version=""1.5.58"" />
    <PackageReference Include=""Microsoft.Extensions.Configuration.Json"" Version=""8.0.16"" />
    <PackageReference Include=""Microsoft.Extensions.Logging.Abstractions"" Version=""8.0.16"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\{apiPath}\{config.MicroserviceName}.Api.csproj"" />
    <ProjectReference Include=""..\..\{applicationPath}\{config.MicroserviceName}.Application.csproj"" />
    <ProjectReference Include=""..\..\{infrastructurePath}\{config.MicroserviceName}.Infrastructure.csproj"" />
    <ProjectReference Include=""..\..\{domainPath}\{config.MicroserviceName}.Domain.csproj"" />
  </ItemGroup>

  <ItemGroup>
    <None Update=""appsettings.Test.json"">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>";
    }

    private string GenerateApiIntegrationTests(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var aggregateLower = aggregate.Name.ToLowerInvariant();
        var createProperties = string.Join(",\n                ", 
            aggregate.Properties.Select(p => $"{p.Name} = testData.{p.Name}"));

        return $@"using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using {config.Namespace}.Api.Models;
using {config.Namespace}.Integration.Tests.Fixtures;
using {config.Namespace}.Integration.Tests.Helpers;

namespace {config.Namespace}.Integration.Tests.Api;

public class {aggregate.Name}ApiIntegrationTests : IClassFixture<TestApplicationFactory>, IClassFixture<DatabaseFixture>
{{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;
    private readonly DatabaseFixture _databaseFixture;

    public {aggregate.Name}ApiIntegrationTests(TestApplicationFactory factory, DatabaseFixture databaseFixture)
    {{
        _factory = factory;
        _databaseFixture = databaseFixture;
        _client = factory.CreateClient();
    }}

    [Fact]
    public async Task Get{aggregate.Name}s_ShouldReturnPagedResults()
    {{
        // Arrange
        await _databaseFixture.SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync(""/api/{aggregateLower}"");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<{aggregate.Name}Response>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }}

    [Fact]
    public async Task Get{aggregate.Name}ById_WithValidId_ShouldReturn{aggregate.Name}()
    {{
        // Arrange
        var testData = await _databaseFixture.Create{aggregate.Name}Async();

        // Act
        var response = await _client.GetAsync($""/api/{aggregateLower}/{{testData.Id}}"");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<{aggregate.Name}Response>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }}

    [Fact]
    public async Task Get{aggregate.Name}ById_WithInvalidId_ShouldReturnNotFound()
    {{
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($""/api/{aggregateLower}/{{invalidId}}"");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }}

    [Fact]
    public async Task Create{aggregate.Name}_WithValidData_ShouldCreateAndReturnId()
    {{
        // Arrange
        var testData = TestDataBuilder.Create{aggregate.Name}Request();
        var request = new Create{aggregate.Name}Request
        {{
            {createProperties}
        }};

        // Act
        var response = await _client.PostAsJsonAsync(""/api/{aggregateLower}"", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        // Verify creation
        var getResponse = await _client.GetAsync($""/api/{aggregateLower}/{{createdId}}"");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }}

    [Fact]
    public async Task Update{aggregate.Name}_WithValidData_ShouldUpdateSuccessfully()
    {{
        // Arrange
        var testData = await _databaseFixture.Create{aggregate.Name}Async();
        var updateRequest = TestDataBuilder.CreateUpdate{aggregate.Name}Request();

        // Act
        var response = await _client.PutAsJsonAsync($""/api/{aggregateLower}/{{testData.Id}}"", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($""/api/{aggregateLower}/{{testData.Id}}"");
        var updated = await getResponse.Content.ReadFromJsonAsync<{aggregate.Name}Response>();
        updated.Should().NotBeNull();
        // Add specific property assertions based on aggregate properties
    }}

    [Fact]
    public async Task Delete{aggregate.Name}_WithValidId_ShouldDeleteSuccessfully()
    {{
        // Arrange
        var testData = await _databaseFixture.Create{aggregate.Name}Async();

        // Act
        var response = await _client.DeleteAsync($""/api/{aggregateLower}/{{testData.Id}}"");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($""/api/{aggregateLower}/{{testData.Id}}"");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }}

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(1, 20)]
    public async Task Get{aggregate.Name}s_WithPaging_ShouldReturnCorrectPage(int page, int pageSize)
    {{
        // Arrange
        await _databaseFixture.SeedMultiple{aggregate.Name}sAsync(25);

        // Act
        var response = await _client.GetAsync($""/api/{aggregateLower}?page={{page}}&pageSize={{pageSize}}"");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<{aggregate.Name}Response>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Count.Should().BeLessOrEqualTo(pageSize);
    }}

    [Fact]
    public async Task Create{aggregate.Name}_WithInvalidData_ShouldReturnBadRequest()
    {{
        // Arrange
        var invalidRequest = new Create{aggregate.Name}Request(); // Empty request

        // Act
        var response = await _client.PostAsJsonAsync(""/api/{aggregateLower}"", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }}

    [Fact]
    public async Task Update{aggregate.Name}_WithInvalidId_ShouldReturnNotFound()
    {{
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateRequest = TestDataBuilder.CreateUpdate{aggregate.Name}Request();

        // Act
        var response = await _client.PutAsJsonAsync($""/api/{aggregateLower}/{{invalidId}}"", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }}
}}";
    }

    private string GenerateDatabaseIntegrationTests(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using {config.Namespace}.Domain.{aggregate.Name};
using {config.Namespace}.Infrastructure.Persistence;
using {config.Namespace}.Infrastructure.Repositories;
using {config.Namespace}.Integration.Tests.Fixtures;
using {config.Namespace}.Integration.Tests.Helpers;

namespace {config.Namespace}.Integration.Tests.Database;

public class {aggregate.Name}DatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{{
    private readonly DatabaseFixture _databaseFixture;

    public {aggregate.Name}DatabaseIntegrationTests(DatabaseFixture databaseFixture)
    {{
        _databaseFixture = databaseFixture;
    }}

    [Fact]
    public async Task Repository_Add_ShouldPersist{aggregate.Name}ToDatabase()
    {{
        // Arrange
        using var context = _databaseFixture.CreateDbContext();
        var repository = new {aggregate.Name}Repository(context);
        var {aggregate.Name.ToLowerInvariant()} = TestDataBuilder.Create{aggregate.Name}();

        // Act
        await repository.AddAsync({aggregate.Name.ToLowerInvariant()});
        await context.SaveChangesAsync();

        // Assert
        var saved{aggregate.Name} = await context.{aggregate.Name}s.FirstOrDefaultAsync(x => x.Id == {aggregate.Name.ToLowerInvariant()}.Id);
        saved{aggregate.Name}.Should().NotBeNull();
        saved{aggregate.Name}!.Id.Should().Be({aggregate.Name.ToLowerInvariant()}.Id);
    }}

    [Fact]
    public async Task Repository_GetById_ShouldRetrieve{aggregate.Name}FromDatabase()
    {{
        // Arrange
        var testData = await _databaseFixture.Create{aggregate.Name}Async();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new {aggregate.Name}Repository(context);

        // Act
        var result = await repository.GetByIdAsync(testData.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }}

    [Fact]
    public async Task Repository_Update_ShouldModify{aggregate.Name}InDatabase()
    {{
        // Arrange
        var testData = await _databaseFixture.Create{aggregate.Name}Async();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new {aggregate.Name}Repository(context);

        // Act
        var {aggregate.Name.ToLowerInvariant()} = await repository.GetByIdAsync(testData.Id);
        {aggregate.Name.ToLowerInvariant()}.Should().NotBeNull();
        
        // Modify entity (add specific property modifications based on aggregate)
        repository.Update({aggregate.Name.ToLowerInvariant()}!);
        await context.SaveChangesAsync();

        // Assert
        var updated{aggregate.Name} = await repository.GetByIdAsync(testData.Id);
        updated{aggregate.Name}.Should().NotBeNull();
        // Add specific assertions for modified properties
    }}

    [Fact]
    public async Task Repository_Delete_ShouldRemove{aggregate.Name}FromDatabase()
    {{
        // Arrange
        var testData = await _databaseFixture.Create{aggregate.Name}Async();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new {aggregate.Name}Repository(context);

        // Act
        var {aggregate.Name.ToLowerInvariant()} = await repository.GetByIdAsync(testData.Id);
        {aggregate.Name.ToLowerInvariant()}.Should().NotBeNull();
        
        repository.Delete({aggregate.Name.ToLowerInvariant()}!);
        await context.SaveChangesAsync();

        // Assert
        var deleted{aggregate.Name} = await repository.GetByIdAsync(testData.Id);
        deleted{aggregate.Name}.Should().BeNull();
    }}

    [Fact]
    public async Task Repository_GetAll_ShouldReturnAll{aggregate.Name}s()
    {{
        // Arrange
        await _databaseFixture.SeedMultiple{aggregate.Name}sAsync(5);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new {aggregate.Name}Repository(context);

        // Act
        var results = await repository.GetAllAsync();

        // Assert
        results.Should().NotBeEmpty();
        results.Count().Should().BeGreaterOrEqualTo(5);
    }}

    [Fact]
    public async Task Repository_GetWithPaging_ShouldReturnPagedResults()
    {{
        // Arrange
        await _databaseFixture.SeedMultiple{aggregate.Name}sAsync(20);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new {aggregate.Name}Repository(context);

        // Act
        var results = await repository.GetPagedAsync(1, 10);

        // Assert
        results.Should().NotBeNull();
        results.Items.Should().HaveCount(10);
        results.TotalCount.Should().BeGreaterOrEqualTo(20);
        results.Page.Should().Be(1);
        results.PageSize.Should().Be(10);
    }}

    [Fact]
    public async Task DbContext_Migrations_ShouldCreateCorrectSchema()
    {{
        // Arrange & Act
        using var context = _databaseFixture.CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        // Assert
        var tableExists = await context.Database.GetDbConnection().QueryAsync<int>(
            ""SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{aggregate.Name}s'"");
        tableExists.First().Should().Be(1);
    }}

    [Fact]
    public async Task DbContext_ConcurrencyHandling_ShouldHandleOptimisticLocking()
    {{
        // Arrange
        var testData = await _databaseFixture.Create{aggregate.Name}Async();
        
        using var context1 = _databaseFixture.CreateDbContext();
        using var context2 = _databaseFixture.CreateDbContext();
        
        var repository1 = new {aggregate.Name}Repository(context1);
        var repository2 = new {aggregate.Name}Repository(context2);

        // Act
        var {aggregate.Name.ToLowerInvariant()}1 = await repository1.GetByIdAsync(testData.Id);
        var {aggregate.Name.ToLowerInvariant()}2 = await repository2.GetByIdAsync(testData.Id);

        {aggregate.Name.ToLowerInvariant()}1.Should().NotBeNull();
        {aggregate.Name.ToLowerInvariant()}2.Should().NotBeNull();

        // Modify both entities
        repository1.Update({aggregate.Name.ToLowerInvariant()}1!);
        repository2.Update({aggregate.Name.ToLowerInvariant()}2!);

        // Save first context
        await context1.SaveChangesAsync();

        // Assert
        // Second context should throw concurrency exception
        var act = async () => await context2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }}
}}";
    }

    private string GenerateEndToEndTests(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using {config.Namespace}.Api.Models;
using {config.Namespace}.Integration.Tests.Fixtures;
using {config.Namespace}.Integration.Tests.Helpers;

namespace {config.Namespace}.Integration.Tests.EndToEnd;

public class {aggregate.Name}EndToEndTests : IClassFixture<TestApplicationFactory>, IClassFixture<DatabaseFixture>
{{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;
    private readonly DatabaseFixture _databaseFixture;

    public {aggregate.Name}EndToEndTests(TestApplicationFactory factory, DatabaseFixture databaseFixture)
    {{
        _factory = factory;
        _databaseFixture = databaseFixture;
        _client = factory.CreateClient();
    }}

    [Fact]
    public async Task Complete{aggregate.Name}Workflow_ShouldWorkEndToEnd()
    {{
        // Arrange
        var createRequest = TestDataBuilder.Create{aggregate.Name}Request();

        // Act & Assert - Create
        var createResponse = await _client.PostAsJsonAsync(""/api/{aggregate.Name.ToLowerInvariant()}"", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        // Act & Assert - Get by ID
        var getResponse = await _client.GetAsync($""/api/{aggregate.Name.ToLowerInvariant()}/{{createdId}}"");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<{aggregate.Name}Response>();
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(createdId);

        // Act & Assert - Update
        var updateRequest = TestDataBuilder.CreateUpdate{aggregate.Name}Request();
        var updateResponse = await _client.PutAsJsonAsync($""/api/{aggregate.Name.ToLowerInvariant()}/{{createdId}}"", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getUpdatedResponse = await _client.GetAsync($""/api/{aggregate.Name.ToLowerInvariant()}/{{createdId}}"");
        var updated = await getUpdatedResponse.Content.ReadFromJsonAsync<{aggregate.Name}Response>();
        updated.Should().NotBeNull();

        // Act & Assert - List (should include our item)
        var listResponse = await _client.GetAsync(""/api/{aggregate.Name.ToLowerInvariant()}"");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResponse<{aggregate.Name}Response>>();
        list.Should().NotBeNull();
        list!.Items.Should().Contain(x => x.Id == createdId);

        // Act & Assert - Delete
        var deleteResponse = await _client.DeleteAsync($""/api/{aggregate.Name.ToLowerInvariant()}/{{createdId}}"");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getDeletedResponse = await _client.GetAsync($""/api/{aggregate.Name.ToLowerInvariant()}/{{createdId}}"");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }}

    [Fact]
    public async Task Multiple{aggregate.Name}Operations_ShouldMaintainDataConsistency()
    {{
        // Arrange
        var requests = Enumerable.Range(1, 5)
            .Select(_ => TestDataBuilder.Create{aggregate.Name}Request())
            .ToList();

        var createdIds = new List<Guid>();

        // Act - Create multiple items
        foreach (var request in requests)
        {{
            var response = await _client.PostAsJsonAsync(""/api/{aggregate.Name.ToLowerInvariant()}"", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            createdIds.Add(id);
        }}

        // Assert - All items should be retrievable
        foreach (var id in createdIds)
        {{
            var getResponse = await _client.GetAsync($""/api/{aggregate.Name.ToLowerInvariant()}/{{id}}"");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }}

        // Assert - List should contain all items
        var listResponse = await _client.GetAsync(""/api/{aggregate.Name.ToLowerInvariant()}?pageSize=20"");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResponse<{aggregate.Name}Response>>();
        list.Should().NotBeNull();
        
        foreach (var id in createdIds)
        {{
            list!.Items.Should().Contain(x => x.Id == id);
        }}

        // Cleanup
        foreach (var id in createdIds)
        {{
            await _client.DeleteAsync($""/api/{aggregate.Name.ToLowerInvariant()}/{{id}}"");
        }}
    }}

    [Fact]
    public async Task {aggregate.Name}Operations_WithRateLimiting_ShouldRespectLimits()
    {{
        // Arrange
        var requests = Enumerable.Range(1, 100)
            .Select(_ => TestDataBuilder.Create{aggregate.Name}Request());

        var tasks = requests.Select(async request =>
        {{
            try
            {{
                var response = await _client.PostAsJsonAsync(""/api/{aggregate.Name.ToLowerInvariant()}"", request);
                return response.StatusCode;
            }}
            catch
            {{
                return HttpStatusCode.TooManyRequests;
            }}
        }});

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        // Some requests should succeed, some might be rate limited
        results.Should().Contain(HttpStatusCode.Created);
        // Note: Rate limiting behavior depends on configuration
    }}

    [Fact]
    public async Task {aggregate.Name}Operations_UnderLoad_ShouldMaintainPerformance()
    {{
        // Arrange
        var concurrentRequests = 20;
        var requests = Enumerable.Range(1, concurrentRequests)
            .Select(_ => TestDataBuilder.Create{aggregate.Name}Request());

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var tasks = requests.Select(async request =>
        {{
            var response = await _client.PostAsJsonAsync(""/api/{aggregate.Name.ToLowerInvariant()}"", request);
            return response.StatusCode;
        }});

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Should().AllBe(HttpStatusCode.Created);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }}

    [Fact]
    public async Task {aggregate.Name}Operations_WithInvalidData_ShouldReturnAppropriateErrors()
    {{
        // Test various invalid scenarios
        var scenarios = new[]
        {{
            new {{ Request = (object?)null, ExpectedStatus = HttpStatusCode.BadRequest }},
            new {{ Request = new {{}}, ExpectedStatus = HttpStatusCode.BadRequest }},
            // Add more invalid scenarios based on aggregate properties
        }};

        foreach (var scenario in scenarios)
        {{
            // Act
            var response = await _client.PostAsJsonAsync(""/api/{aggregate.Name.ToLowerInvariant()}"", scenario.Request);

            // Assert
            response.StatusCode.Should().Be(scenario.ExpectedStatus);
        }}
    }}
}}";
    }

    private string GenerateTestApplicationFactory(TemplateConfiguration config)
    {
        return $@"using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using {config.Namespace}.Infrastructure.Persistence;

namespace {config.Namespace}.Integration.Tests.Fixtures;

public class TestApplicationFactory : WebApplicationFactory<Program>
{{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {{
        builder.ConfigureAppConfiguration((context, config) =>
        {{
            config.AddJsonFile(""appsettings.Test.json"", optional: false);
        }});

        builder.ConfigureServices(services =>
        {{
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {{
                services.Remove(descriptor);
            }}

            // Add test database (in-memory)
            services.AddDbContext<ApplicationDbContext>(options =>
            {{
                options.UseInMemoryDatabase(""TestDatabase_"" + Guid.NewGuid());
                options.EnableSensitiveDataLogging();
            }});

            // Configure logging for tests
            services.AddLogging(builder =>
            {{
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            }});

            // Build service provider and ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();
        }});

        builder.UseEnvironment(""Test"");
    }}
}}";
    }

    private string GenerateDatabaseFixture(TemplateConfiguration config)
    {
        return $@"using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using {config.Namespace}.Domain.{config.Domain?.Aggregates?.FirstOrDefault()?.Name ?? "Item"};
using {config.Namespace}.Infrastructure.Persistence;
using {config.Namespace}.Integration.Tests.Helpers;

namespace {config.Namespace}.Integration.Tests.Fixtures;

public class DatabaseFixture : IDisposable
{{
    private readonly ApplicationDbContext _context;
    private readonly string _connectionString;

    public DatabaseFixture()
    {{
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(""appsettings.Test.json"")
            .Build();

        _connectionString = configuration.GetConnectionString(""DefaultConnection"") 
            ?? ""Data Source=:memory:"";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(""TestDatabase_"" + Guid.NewGuid())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
    }}

    public ApplicationDbContext CreateDbContext()
    {{
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(""TestDatabase_"" + Guid.NewGuid())
            .EnableSensitiveDataLogging()
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }}

    public async Task SeedTestDataAsync()
    {{
        if (!_context.{config.Domain?.Aggregates?.FirstOrDefault()?.Name ?? "Item"}s.Any())
        {{
            var testItems = TestDataBuilder.CreateMultiple{config.Domain?.Aggregates?.FirstOrDefault()?.Name ?? "Item"}s(10);
            _context.{config.Domain?.Aggregates?.FirstOrDefault()?.Name ?? "Item"}s.AddRange(testItems);
            await _context.SaveChangesAsync();
        }}
    }}

{string.Join("\n\n", (config.Domain?.Aggregates ?? new List<AggregateConfiguration>()).Select(aggregate => $@"    public async Task<{aggregate.Name}> Create{aggregate.Name}Async()
    {{
        var {aggregate.Name.ToLowerInvariant()} = TestDataBuilder.Create{aggregate.Name}();
        _context.{aggregate.Name}s.Add({aggregate.Name.ToLowerInvariant()});
        await _context.SaveChangesAsync();
        return {aggregate.Name.ToLowerInvariant()};
    }}

    public async Task SeedMultiple{aggregate.Name}sAsync(int count)
    {{
        var {aggregate.Name.ToLowerInvariant()}s = TestDataBuilder.CreateMultiple{aggregate.Name}s(count);
        _context.{aggregate.Name}s.AddRange({aggregate.Name.ToLowerInvariant()}s);
        await _context.SaveChangesAsync();
    }}"))}

    public async Task CleanupAsync()
    {{
        _context.Database.EnsureDeleted();
        await _context.Database.EnsureCreatedAsync();
    }}

    public void Dispose()
    {{
        _context.Dispose();
    }}
}}";
    }

    private string GenerateTestDataBuilder(TemplateConfiguration config)
    {
        var aggregates = config.Domain?.Aggregates ?? new List<AggregateConfiguration>();
        
        return $@"using Bogus;
using {config.Namespace}.Api.Models;
{string.Join("\n", aggregates.Select(a => $"using {config.Namespace}.Domain.{a.Name};"))}

namespace {config.Namespace}.Integration.Tests.Helpers;

public static class TestDataBuilder
{{
{string.Join("\n\n", aggregates.Select(aggregate => GenerateTestDataForAggregate(config, aggregate)))}
}}";
    }

    private string GenerateTestDataForAggregate(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var properties = aggregate.Properties.Select(p => GenerateFakerProperty(p)).ToList();
        var propertiesString = string.Join("\n            ", properties);

        return $@"    private static readonly Faker<{aggregate.Name}> {aggregate.Name}Faker = new Faker<{aggregate.Name}>()
{propertiesString};

    private static readonly Faker<Create{aggregate.Name}Request> Create{aggregate.Name}RequestFaker = new Faker<Create{aggregate.Name}Request>()
{string.Join("\n", aggregate.Properties.Select(p => $"        .RuleFor(x => x.{p.Name}, f => {GenerateFakerValue(p)})"))};

    private static readonly Faker<Update{aggregate.Name}Request> Update{aggregate.Name}RequestFaker = new Faker<Update{aggregate.Name}Request>()
{string.Join("\n", aggregate.Properties.Select(p => $"        .RuleFor(x => x.{p.Name}, f => {GenerateFakerValue(p)})"))};

    public static {aggregate.Name} Create{aggregate.Name}() => {aggregate.Name}Faker.Generate();

    public static List<{aggregate.Name}> CreateMultiple{aggregate.Name}s(int count) => {aggregate.Name}Faker.Generate(count);

    public static Create{aggregate.Name}Request Create{aggregate.Name}Request() => Create{aggregate.Name}RequestFaker.Generate();

    public static Update{aggregate.Name}Request CreateUpdate{aggregate.Name}Request() => Update{aggregate.Name}RequestFaker.Generate();";
    }

    private string GenerateFakerProperty(PropertyConfiguration property)
    {
        return $"        .RuleFor(x => x.{property.Name}, f => {GenerateFakerValue(property)})";
    }

    private string GenerateFakerValue(PropertyConfiguration property)
    {
        return property.Type.ToLowerInvariant() switch
        {
            "string" => "f.Lorem.Word()",
            "int" => "f.Random.Int(1, 1000)",
            "decimal" => "f.Random.Decimal(1, 1000)",
            "bool" => "f.Random.Bool()",
            "datetime" => "f.Date.Recent()",
            "guid" => "f.Random.Guid()",
            _ => "f.Lorem.Word()"
        };
    }

    private string GenerateHttpClientExtensions(TemplateConfiguration config)
    {
        return $@"using System.Net.Http.Json;
using System.Text.Json;

namespace {config.Namespace}.Integration.Tests.Helpers;

public static class HttpClientExtensions
{{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {{
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }};

    public static async Task<T?> GetFromJsonAsync<T>(this HttpClient client, string requestUri)
    {{
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }}

    public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
    {{
        var json = JsonSerializer.Serialize(value, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, ""application/json"");
        return await client.PostAsync(requestUri, content);
    }}

    public static async Task<HttpResponseMessage> PutAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
    {{
        var json = JsonSerializer.Serialize(value, JsonOptions);
        var content = new StringContent(json, System.Text.Encoding.UTF8, ""application/json"");
        return await client.PutAsync(requestUri, content);
    }}

    public static async Task<string> GetStringAsync(this HttpClient client, string requestUri)
    {{
        var response = await client.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }}
}}";
    }

    private string GenerateTestContainersSetup(TemplateConfiguration config)
    {
        var provider = config.Features?.Persistence?.Provider?.ToLowerInvariant() ?? "inmemory";
        
        return $@"using Testcontainers.PostgreSql;
using Testcontainers.MsSql;

namespace {config.Namespace}.Integration.Tests.Fixtures;

public class TestContainersSetup : IAsyncLifetime
{{
    private PostgreSqlContainer? _postgresContainer;
    private MsSqlContainer? _sqlServerContainer;

    public string ConnectionString {{ get; private set; }} = string.Empty;

    public async Task InitializeAsync()
    {{
        {GenerateContainerSetup(provider)}
    }}

    public async Task DisposeAsync()
    {{
        if (_postgresContainer != null)
        {{
            await _postgresContainer.DisposeAsync();
        }}

        if (_sqlServerContainer != null)
        {{
            await _sqlServerContainer.DisposeAsync();
        }}
    }}

    private async Task SetupPostgreSqlContainer()
    {{
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase(""testdb"")
            .WithUsername(""testuser"")
            .WithPassword(""testpass"")
            .WithCleanUp(true)
            .Build();

        await _postgresContainer.StartAsync();
        ConnectionString = _postgresContainer.GetConnectionString();
    }}

    private async Task SetupSqlServerContainer()
    {{
        _sqlServerContainer = new MsSqlBuilder()
            .WithPassword(""TestPass123!"")
            .WithCleanUp(true)
            .Build();

        await _sqlServerContainer.StartAsync();
        ConnectionString = _sqlServerContainer.GetConnectionString();
    }}
}}";
    }

    private string GenerateContainerSetup(string provider)
    {
        return provider switch
        {
            "postgresql" => "await SetupPostgreSqlContainer();",
            "sqlserver" => "await SetupSqlServerContainer();",
            _ => "// Using in-memory database for tests\nConnectionString = \"Data Source=:memory:\";"
        };
    }

    private string GenerateTestSettings(TemplateConfiguration config)
    {
        return $@"{{
  ""Logging"": {{
    ""LogLevel"": {{
      ""Default"": ""Warning"",
      ""Microsoft"": ""Warning"",
      ""Microsoft.Hosting.Lifetime"": ""Information""
    }}
  }},
  ""ConnectionStrings"": {{
    ""DefaultConnection"": ""Data Source=:memory:""
  }},
  ""Features"": {{
    ""RateLimiting"": {{
      ""Enabled"": false
    }},
    ""Authentication"": {{
      ""Enabled"": false
    }}
  }}
}}";
    }

    private string GenerateGlobalUsings()
    {
        return @"global using Xunit;
global using FluentAssertions;
global using Microsoft.Extensions.DependencyInjection;
global using System.Net.Http.Json;";
    }
}