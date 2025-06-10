using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.Tests;

public class UnitTestModule : ITemplateModule
{
    public string Name => "UnitTests";
    public string Description => "Generates comprehensive unit tests for Domain, Application, and Infrastructure layers";

    public bool IsEnabled(TemplateConfiguration config)
    {
        // Always generate tests - they are essential
        return true;
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var outputPath = context.GetTestsProjectPath();

        // Create test project structure
        await CreateTestProjectStructureAsync(outputPath, config, context);

        // Generate tests for each aggregate
        if (config.Domain?.Aggregates != null)
        {
            foreach (var aggregate in config.Domain.Aggregates)
            {
                await GenerateAggregateTestsAsync(outputPath, config, aggregate, context);
            }
        }

        // Generate test utilities and fixtures
        await GenerateTestUtilitiesAsync(outputPath, config, context);
    }

    private async Task CreateTestProjectStructureAsync(string outputPath, TemplateConfiguration config, GenerationContext context)
    {
        Directory.CreateDirectory(outputPath);
        Directory.CreateDirectory(Path.Combine(outputPath, "Domain"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Domain", "Entities"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Domain", "Events"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Application"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Application", "Commands"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Application", "Queries"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Infrastructure"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Utilities"));
        Directory.CreateDirectory(Path.Combine(outputPath, "Builders"));

        // Generate test project file
        var csprojContent = GenerateTestProjectFile(config);
        await context.WriteFileAsync($"{outputPath}/{config.MicroserviceName}.Tests.csproj", csprojContent);

        // Generate global usings
        var globalUsingsContent = GenerateGlobalUsings();
        await context.WriteFileAsync($"{outputPath}/GlobalUsings.cs", globalUsingsContent);
    }

    private async Task GenerateAggregateTestsAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate, GenerationContext context)
    {
        // Generate Domain Entity Tests
        await GenerateDomainEntityTestsAsync(outputPath, config, aggregate, context);

        // Generate Domain Event Tests
        await GenerateDomainEventTestsAsync(outputPath, config, aggregate, context);

        // Generate Application Command Tests
        await GenerateCommandTestsAsync(outputPath, config, aggregate, context);

        // Generate Application Query Tests
        await GenerateQueryTestsAsync(outputPath, config, aggregate, context);

        // Generate Test Builders
        await GenerateTestBuildersAsync(outputPath, config, aggregate, context);
    }

    private async Task GenerateDomainEntityTestsAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate, GenerationContext context)
    {
        var testContent = GenerateDomainEntityTests(config, aggregate);
        await context.WriteFileAsync($"{outputPath}/Domain/Entities/{aggregate.Name}Tests.cs", testContent);
    }

    private async Task GenerateDomainEventTestsAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate, GenerationContext context)
    {
        var testContent = GenerateDomainEventTests(config, aggregate);
        await context.WriteFileAsync($"{outputPath}/Domain/Events/{aggregate.Name}EventTests.cs", testContent);
    }

    private async Task GenerateCommandTestsAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate, GenerationContext context)
    {
        // Generate tests for CRUD commands
        var crudOperations = new[] { "Create", "Update", "Delete" };
        var customOperations = aggregate.Operations ?? new List<string>();
        var allOperations = crudOperations.Concat(customOperations).Distinct();

        foreach (var operation in allOperations)
        {
            var testContent = GenerateCommandHandlerTests(config, aggregate, operation);
            await context.WriteFileAsync($"{outputPath}/Application/Commands/{operation}{aggregate.Name}CommandHandlerTests.cs", testContent);
        }
    }

    private async Task GenerateQueryTestsAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate, GenerationContext context)
    {
        var queries = new[]
        {
            $"Get{aggregate.Name}ById",
            $"Get{aggregate.Name}s",
            $"Get{aggregate.Name}sWithPaging"
        };

        foreach (var queryName in queries)
        {
            var testContent = GenerateQueryHandlerTests(config, aggregate, queryName);
            await context.WriteFileAsync($"{outputPath}/Application/Queries/{queryName}QueryHandlerTests.cs", testContent);
        }
    }

    private async Task GenerateTestBuildersAsync(string outputPath, TemplateConfiguration config, AggregateConfiguration aggregate, GenerationContext context)
    {
        var builderContent = GenerateTestBuilder(config, aggregate);
        await context.WriteFileAsync($"{outputPath}/Builders/{aggregate.Name}Builder.cs", builderContent);
    }

    private async Task GenerateTestUtilitiesAsync(string outputPath, TemplateConfiguration config, GenerationContext context)
    {
        // Generate Mock Repository
        var mockRepositoryContent = GenerateMockRepository(config);
        await context.WriteFileAsync($"{outputPath}/Utilities/MockRepository.cs", mockRepositoryContent);

        // Generate Test Data
        var testDataContent = GenerateTestData(config);
        await context.WriteFileAsync($"{outputPath}/Utilities/TestData.cs", testDataContent);

        // Generate Test Extensions
        var testExtensionsContent = GenerateTestExtensions(config);
        await context.WriteFileAsync($"{outputPath}/Utilities/TestExtensions.cs", testExtensionsContent);
    }

    private string GenerateTestProjectFile(TemplateConfiguration config)
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
    <PackageReference Include=""xunit.runner.visualstudio"" Version=""2.5.3"">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include=""FluentAssertions"" Version=""6.12.0"" />
    <PackageReference Include=""Moq"" Version=""4.20.69"" />
    <PackageReference Include=""AutoFixture"" Version=""4.18.0"" />
    <PackageReference Include=""AutoFixture.Xunit2"" Version=""4.18.0"" />
    <PackageReference Include=""Microsoft.Extensions.Logging.Abstractions"" Version=""8.0.0"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\{domainPath}\{config.MicroserviceName}.Domain.csproj"" />
    <ProjectReference Include=""..\..\{applicationPath}\{config.MicroserviceName}.Application.csproj"" />
    <ProjectReference Include=""..\..\{infrastructurePath}\{config.MicroserviceName}.Infrastructure.csproj"" />
  </ItemGroup>

</Project>";
    }

    private string GenerateGlobalUsings()
    {
        return @"global using Xunit;
global using FluentAssertions;
global using Moq;
global using AutoFixture;
global using AutoFixture.Xunit2;
global using Microsoft.Extensions.Logging;";
    }

    private string GenerateDomainEntityTests(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var constructorParams = string.Join(", ", aggregate.Properties.Select(p => GetTestValue(p.Type)));
        var propertyAssertions = string.Join("\n        ", aggregate.Properties.Select(p => 
            $"entity.{p.Name}.Should().Be({GetTestValue(p.Type)});"));

        return $@"using {config.Namespace}.Domain.Entities;
using {config.Namespace}.Domain.Events;
using {config.Namespace}.UnitTests.Builders;

namespace {config.Namespace}.UnitTests.Domain.Entities;

public class {aggregate.Name}Tests
{{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateEntity()
    {{
        // Arrange
{string.Join("\n        ", aggregate.Properties.Select(p => $"var {ToCamelCase(p.Name)} = {GetTestValue(p.Type)};"))}

        // Act
        var entity = new {aggregate.Name}({string.Join(", ", aggregate.Properties.Select(p => ToCamelCase(p.Name)))});

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        {propertyAssertions}
    }}

    [Fact]
    public void Constructor_ShouldRaise{aggregate.Name}CreatedEvent()
    {{
        // Arrange
{string.Join("\n        ", aggregate.Properties.Select(p => $"var {ToCamelCase(p.Name)} = {GetTestValue(p.Type)};"))}

        // Act
        var entity = new {aggregate.Name}({string.Join(", ", aggregate.Properties.Select(p => ToCamelCase(p.Name)))});

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<{aggregate.Name}CreatedEvent>();
    }}

{GenerateOperationTests(config, aggregate)}

    [Theory]
    [AutoData]
    public void Constructor_WithBuilder_ShouldCreateValidEntity({aggregate.Name}Builder builder)
    {{
        // Act
        var entity = builder.Build();

        // Assert
        entity.Should().NotBeNull();
        entity.Id.Should().NotBeEmpty();
        entity.DomainEvents.Should().NotBeEmpty();
    }}

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {{
        // Arrange
        var entity = new {aggregate.Name}Builder().Build();
        entity.DomainEvents.Should().NotBeEmpty();

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }}
}}";
    }

    private string GenerateOperationTests(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        if (aggregate.Operations == null || !aggregate.Operations.Any())
            return "";

        var tests = new List<string>();
        
        foreach (var operation in aggregate.Operations)
        {
            tests.Add($@"
    [Fact]
    public void {operation}_ShouldExecuteSuccessfully()
    {{
        // Arrange
        var entity = new {aggregate.Name}Builder().Build();
        var initialEventCount = entity.DomainEvents.Count;

        // Act
        entity.{operation}();

        // Assert
        // Add specific assertions based on operation logic
        entity.DomainEvents.Should().HaveCountGreaterThan(initialEventCount);
    }}");
        }

        return string.Join("", tests);
    }

    private string GenerateDomainEventTests(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        return $@"using {config.Namespace}.Domain.Events;

namespace {config.Namespace}.UnitTests.Domain.Events;

public class {aggregate.Name}EventTests
{{
    [Fact]
    public void {aggregate.Name}CreatedEvent_ShouldHaveCorrectProperties()
    {{
        // Arrange
        var aggregateId = Guid.NewGuid();
{string.Join("\n        ", aggregate.Properties.Select(p => $"var {ToCamelCase(p.Name)} = {GetTestValue(p.Type)};"))}

        // Act
        var domainEvent = new {aggregate.Name}CreatedEvent(aggregateId, {string.Join(", ", aggregate.Properties.Select(p => ToCamelCase(p.Name)))});

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.{aggregate.Name}Id.Should().Be(aggregateId);
{string.Join("\n        ", aggregate.Properties.Select(p => $"domainEvent.{p.Name}.Should().Be({ToCamelCase(p.Name)});"))}
        domainEvent.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }}

    [Theory]
    [AutoData]
    public void {aggregate.Name}CreatedEvent_WithAutoData_ShouldBeValid(
        Guid aggregateId,
{string.Join(",\n        ", aggregate.Properties.Select(p => $"{p.Type} {ToCamelCase(p.Name)}"))}
    )
    {{
        // Act
        var domainEvent = new {aggregate.Name}CreatedEvent(aggregateId, {string.Join(", ", aggregate.Properties.Select(p => ToCamelCase(p.Name)))});

        // Assert
        domainEvent.Should().NotBeNull();
        domainEvent.{aggregate.Name}Id.Should().Be(aggregateId);
    }}
}}";
    }

    private string GenerateCommandHandlerTests(TemplateConfiguration config, AggregateConfiguration aggregate, string operation)
    {
        var handlerName = $"{operation}{aggregate.Name}CommandHandler";
        var commandName = $"{operation}{aggregate.Name}Command";
        var returnType = operation.ToLowerInvariant() == "create" ? "Guid" : "void";
        var asyncReturn = operation.ToLowerInvariant() == "create" ? "Task<Guid>" : "Task";

        return $@"using {config.Namespace}.Application.{aggregate.Name}.Commands.{operation}{aggregate.Name};
using {config.Namespace}.Application.Common;
using {config.Namespace}.Domain.Entities;
using {config.Namespace}.UnitTests.Builders;
using {config.Namespace}.UnitTests.Utilities;

namespace {config.Namespace}.UnitTests.Application.Commands;

public class {handlerName}Tests
{{
    private readonly Mock<IRepository<{aggregate.Name}>> _mockRepository;
    private readonly Mock<ILogger<{handlerName}>> _mockLogger;
    private readonly {handlerName} _handler;
    private readonly Fixture _fixture;

    public {handlerName}Tests()
    {{
        _mockRepository = new Mock<IRepository<{aggregate.Name}>>();
        _mockLogger = new Mock<ILogger<{handlerName}>>();
        _handler = new {handlerName}(_mockRepository.Object);
        _fixture = new Fixture();
    }}

{GenerateCommandHandlerTestMethods(config, aggregate, operation, commandName, returnType, asyncReturn)}
}}";
    }

    private string GenerateCommandHandlerTestMethods(TemplateConfiguration config, AggregateConfiguration aggregate, string operation, string commandName, string returnType, string asyncReturn)
    {
        return operation.ToLowerInvariant() switch
        {
            "create" => GenerateCreateCommandTests(aggregate, commandName),
            "update" => GenerateUpdateCommandTests(aggregate, commandName),
            "delete" => GenerateDeleteCommandTests(aggregate, commandName),
            _ => GenerateCustomCommandTests(aggregate, operation, commandName)
        };
    }

    private string GenerateCreateCommandTests(AggregateConfiguration aggregate, string commandName)
    {
        return $@"
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateEntityAndReturnId()
    {{
        // Arrange
        var command = new {commandName}({string.Join(", ", aggregate.Properties.Select(p => GetTestValue(p.Type)))});

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<{aggregate.Name}>(), It.IsAny<CancellationToken>()), Times.Once);
    }}

    [Theory]
    [AutoData]
    public async Task Handle_WithAutoDataCommand_ShouldCreateEntity({commandName} command)
    {{
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockRepository.Verify(x => x.AddAsync(It.Is<{aggregate.Name}>(e => 
{string.Join(" &&\n            ", aggregate.Properties.Select(p => $"e.{p.Name} == command.{ToPascalCase(p.Name)}"))}
        ), It.IsAny<CancellationToken>()), Times.Once);
    }}

    [Fact]
    public async Task Handle_WhenRepositoryThrows_ShouldPropagateException()
    {{
        // Arrange
        var command = new {commandName}({string.Join(", ", aggregate.Properties.Select(p => GetTestValue(p.Type)))});
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<{aggregate.Name}>(), It.IsAny<CancellationToken>()))
                      .ThrowsAsync(new InvalidOperationException(""Database error""));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
    }}";
    }

    private string GenerateUpdateCommandTests(AggregateConfiguration aggregate, string commandName)
    {
        return $@"
    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateEntity()
    {{
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new {aggregate.Name}Builder().WithId(entityId).Build();
        var command = new {commandName}(entityId, {string.Join(", ", aggregate.Properties.Select(p => GetTestValue(p.Type)))});

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingEntity);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(existingEntity, It.IsAny<CancellationToken>()), Times.Once);
    }}

    [Fact]
    public async Task Handle_WithNonExistentEntity_ShouldThrowNotFoundException()
    {{
        // Arrange
        var entityId = Guid.NewGuid();
        var command = new {commandName}(entityId, {string.Join(", ", aggregate.Properties.Select(p => GetTestValue(p.Type)))});

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((({aggregate.Name}?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }}";
    }

    private string GenerateDeleteCommandTests(AggregateConfiguration aggregate, string commandName)
    {
        return $@"
    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteEntity()
    {{
        // Arrange
        var entityId = Guid.NewGuid();
        var existingEntity = new {aggregate.Name}Builder().WithId(entityId).Build();
        var command = new {commandName}(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(existingEntity);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(existingEntity, It.IsAny<CancellationToken>()), Times.Once);
    }}

    [Fact]
    public async Task Handle_WithNonExistentEntity_ShouldThrowNotFoundException()
    {{
        // Arrange
        var entityId = Guid.NewGuid();
        var command = new {commandName}(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((({aggregate.Name}?)null));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }}";
    }

    private string GenerateCustomCommandTests(AggregateConfiguration aggregate, string operation, string commandName)
    {
        return $@"
    [Fact]
    public async Task Handle_WithValidCommand_ShouldExecute{operation}Successfully()
    {{
        // Arrange
        var command = new {commandName}({string.Join(", ", aggregate.Properties.Take(2).Select(p => GetTestValue(p.Type)))});

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Add specific assertions for {operation} operation
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<{aggregate.Name}>(), It.IsAny<CancellationToken>()), Times.Once);
    }}";
    }

    private string GenerateQueryHandlerTests(TemplateConfiguration config, AggregateConfiguration aggregate, string queryName)
    {
        var handlerName = $"{queryName}QueryHandler";
        var queryTypeName = $"{queryName}Query";

        return $@"using {config.Namespace}.Application.{aggregate.Name}.Queries.{queryName};
using {config.Namespace}.Application.{aggregate.Name}.DTOs;
using {config.Namespace}.Application.Common;
using {config.Namespace}.Domain.Entities;
using {config.Namespace}.UnitTests.Builders;

namespace {config.Namespace}.UnitTests.Application.Queries;

public class {handlerName}Tests
{{
    private readonly Mock<IRepository<{aggregate.Name}>> _mockRepository;
    private readonly {handlerName} _handler;
    private readonly Fixture _fixture;

    public {handlerName}Tests()
    {{
        _mockRepository = new Mock<IRepository<{aggregate.Name}>>();
        _handler = new {handlerName}(_mockRepository.Object);
        _fixture = new Fixture();
    }}

{GenerateQueryHandlerTestMethods(aggregate, queryName, queryTypeName)}
}}";
    }

    private string GenerateQueryHandlerTestMethods(AggregateConfiguration aggregate, string queryName, string queryTypeName)
    {
        if (queryName.Contains("ById"))
        {
            return GenerateGetByIdQueryTests(aggregate, queryTypeName);
        }
        else if (queryName.Contains("WithPaging"))
        {
            return GenerateGetWithPagingQueryTests(aggregate, queryTypeName);
        }
        else
        {
            return GenerateGetAllQueryTests(aggregate, queryTypeName);
        }
    }

    private string GenerateGetByIdQueryTests(AggregateConfiguration aggregate, string queryTypeName)
    {
        return $@"
    [Fact]
    public async Task Handle_WithExistingEntity_ShouldReturnDto()
    {{
        // Arrange
        var entityId = Guid.NewGuid();
        var entity = new {aggregate.Name}Builder().WithId(entityId).Build();
        var query = new {queryTypeName}(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entity);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(entityId);
    }}

    [Fact]
    public async Task Handle_WithNonExistentEntity_ShouldReturnNull()
    {{
        // Arrange
        var entityId = Guid.NewGuid();
        var query = new {queryTypeName}(entityId);

        _mockRepository.Setup(x => x.GetByIdAsync(entityId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync((({aggregate.Name}?)null));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }}";
    }

    private string GenerateGetAllQueryTests(AggregateConfiguration aggregate, string queryTypeName)
    {
        return $@"
    [Fact]
    public async Task Handle_WithExistingEntities_ShouldReturnDtos()
    {{
        // Arrange
        var entities = new List<{aggregate.Name}>
        {{
            new {aggregate.Name}Builder().Build(),
            new {aggregate.Name}Builder().Build(),
            new {aggregate.Name}Builder().Build()
        }};
        var query = new {queryTypeName}();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(entities);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().AllSatisfy(dto => dto.Id.Should().NotBeEmpty());
    }}

    [Fact]
    public async Task Handle_WithNoEntities_ShouldReturnEmptyList()
    {{
        // Arrange
        var query = new {queryTypeName}();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new List<{aggregate.Name}>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }}";
    }

    private string GenerateGetWithPagingQueryTests(AggregateConfiguration aggregate, string queryTypeName)
    {
        return $@"
    [Fact]
    public async Task Handle_WithValidPaging_ShouldReturnPagedResult()
    {{
        // Arrange
        var entities = Enumerable.Range(1, 25)
            .Select(_ => new {aggregate.Name}Builder().Build())
            .ToList();
        
        var pagedResult = new PagedResult<{aggregate.Name}>
        {{
            Items = entities.Take(10).ToList(),
            TotalCount = 25,
            Page = 1,
            PageSize = 10
        }};

        var query = new {queryTypeName}(1, 10);

        _mockRepository.Setup(x => x.GetPagedAsync(1, 10, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(pagedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(3);
    }}

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(-1, 10)]
    [InlineData(1, -5)]
    public async Task Handle_WithInvalidPaging_ShouldThrowArgumentException(int page, int pageSize)
    {{
        // Arrange
        var query = new {queryTypeName}(page, pageSize);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(query, CancellationToken.None));
    }}";
    }

    private string GenerateTestBuilder(TemplateConfiguration config, AggregateConfiguration aggregate)
    {
        var properties = string.Join("\n    ", aggregate.Properties.Select(p => 
            $"private {p.Type} _{ToCamelCase(p.Name)} = {GetTestValue(p.Type)};"));

        var withMethods = string.Join("\n\n    ", aggregate.Properties.Select(p => 
            $"public {aggregate.Name}Builder With{p.Name}({p.Type} {ToCamelCase(p.Name)})\n    {{\n        _{ToCamelCase(p.Name)} = {ToCamelCase(p.Name)};\n        return this;\n    }}"));

        return $@"using {config.Namespace}.Domain.Entities;

namespace {config.Namespace}.UnitTests.Builders;

public class {aggregate.Name}Builder
{{
    private Guid _id = Guid.NewGuid();
{properties}

    public {aggregate.Name}Builder WithId(Guid id)
    {{
        _id = id;
        return this;
    }}

{withMethods}

    public {aggregate.Name} Build()
    {{
        var entity = new {aggregate.Name}({string.Join(", ", aggregate.Properties.Select(p => $"_{ToCamelCase(p.Name)}"))});
        
        // Use reflection to set the Id if needed
        var idProperty = typeof({aggregate.Name}).GetProperty(""Id"");
        if (idProperty != null && idProperty.CanWrite)
        {{
            idProperty.SetValue(entity, _id);
        }}
        
        return entity;
    }}

    public static implicit operator {aggregate.Name}({aggregate.Name}Builder builder)
    {{
        return builder.Build();
    }}
}}";
    }

    private string GenerateMockRepository(TemplateConfiguration config)
    {
        return $@"using {config.Namespace}.Application.Common;
using System.Linq.Expressions;

namespace {config.Namespace}.UnitTests.Utilities;

public class MockRepository<T> : Mock<IRepository<T>> where T : class
{{
    private readonly List<T> _entities = new();

    public MockRepository()
    {{
        Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => _entities.AsReadOnly());

        Setup(x => x.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) => _entities.Add(entity))
            .Returns(Task.CompletedTask);

        Setup(x => x.UpdateAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Setup(x => x.DeleteAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
            .Callback<T, CancellationToken>((entity, _) => _entities.Remove(entity))
            .Returns(Task.CompletedTask);
    }}

    public void AddEntity(T entity)
    {{
        _entities.Add(entity);
    }}

    public void AddEntities(IEnumerable<T> entities)
    {{
        _entities.AddRange(entities);
    }}

    public void ClearEntities()
    {{
        _entities.Clear();
    }}

    public IReadOnlyList<T> GetEntities()
    {{
        return _entities.AsReadOnly();
    }}
}}";
    }

    private string GenerateTestData(TemplateConfiguration config)
    {
        return $@"namespace {config.Namespace}.UnitTests.Utilities;

public static class TestData
{{
    public static class Guids
    {{
        public static readonly Guid Valid = Guid.Parse(""12345678-1234-1234-1234-123456789012"");
        public static readonly Guid Another = Guid.Parse(""87654321-4321-4321-4321-210987654321"");
        public static readonly Guid Empty = Guid.Empty;
    }}

    public static class Strings
    {{
        public const string Valid = ""Test String"";
        public const string Long = ""This is a very long string that might be used for testing maximum length validations and other string-related scenarios"";
        public const string Empty = """";
        public const string WhiteSpace = ""   "";
    }}

    public static class Numbers
    {{
        public const int ValidInt = 42;
        public const int Zero = 0;
        public const int Negative = -1;
        public const decimal ValidDecimal = 123.45m;
        public const decimal ZeroDecimal = 0m;
        public const decimal NegativeDecimal = -123.45m;
    }}

    public static class Dates
    {{
        public static readonly DateTime Valid = new(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime Past = new(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime Future = new(2030, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    }}
}}";
    }

    private string GenerateTestExtensions(TemplateConfiguration config)
    {
        return $@"using System.Reflection;

namespace {config.Namespace}.UnitTests.Utilities;

public static class TestExtensions
{{
    public static T SetPrivateProperty<T>(this T obj, string propertyName, object value)
    {{
        var property = typeof(T).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (property != null)
        {{
            property.SetValue(obj, value);
        }}
        return obj;
    }}

    public static object? GetPrivateProperty<T>(this T obj, string propertyName)
    {{
        var property = typeof(T).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        return property?.GetValue(obj);
    }}

    public static T SetPrivateField<T>(this T obj, string fieldName, object value)
    {{
        var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {{
            field.SetValue(obj, value);
        }}
        return obj;
    }}

    public static object? GetPrivateField<T>(this T obj, string fieldName)
    {{
        var field = typeof(T).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        return field?.GetValue(obj);
    }}

    public static void ShouldRaiseDomainEvent<TEvent>(this AggregateKit.AggregateRoot<Guid> aggregate)
        where TEvent : AggregateKit.DomainEventBase
    {{
        aggregate.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TEvent>();
    }}

    public static void ShouldRaiseDomainEvents<TEvent>(this AggregateKit.AggregateRoot<Guid> aggregate, int expectedCount)
        where TEvent : AggregateKit.DomainEventBase
    {{
        aggregate.DomainEvents.OfType<TEvent>().Should().HaveCount(expectedCount);
    }}
}}";
    }

    // Helper methods
    private static string GetTestValue(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "string" => "\"Test Value\"",
            "guid" => "Guid.NewGuid()",
            "int" => "42",
            "decimal" => "123.45m",
            "bool" => "true",
            "datetime" => "DateTime.UtcNow",
            _ => $"default({type})"
        };
    }

    private static string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToLowerInvariant(input[0]) + input[1..];
    }

    private static string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpperInvariant(input[0]) + input[1..];
    }
} 