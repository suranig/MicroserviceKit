using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using TestService.Api.Models;
using TestService.Integration.Tests.Fixtures;
using TestService.Integration.Tests.Helpers;

namespace TestService.Integration.Tests.Api;

public class TestApiIntegrationTests : IClassFixture<TestApplicationFactory>, IClassFixture<DatabaseFixture>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;
    private readonly DatabaseFixture _databaseFixture;

    public TestApiIntegrationTests(TestApplicationFactory factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTests_ShouldReturnPagedResults()
    {
        // Arrange
        await _databaseFixture.SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/test");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<TestResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetTestById_WithValidId_ShouldReturnTest()
    {
        // Arrange
        var testData = await _databaseFixture.CreateTestAsync();

        // Act
        var response = await _client.GetAsync($"/api/test/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TestResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task GetTestById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/test/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTest_WithValidData_ShouldCreateAndReturnId()
    {
        // Arrange
        var testData = TestDataBuilder.CreateTestRequest();
        var request = new CreateTestRequest
        {
            Id = testData.Id,
                Name = testData.Name,
                Description = testData.Description,
                CreatedAt = testData.CreatedAt,
                UpdatedAt = testData.UpdatedAt
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/test", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        // Verify creation
        var getResponse = await _client.GetAsync($"/api/test/{createdId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateTest_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateTestAsync();
        var updateRequest = TestDataBuilder.CreateUpdateTestRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/test/{testData.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/test/{testData.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<TestResponse>();
        updated.Should().NotBeNull();
        // Add specific property assertions based on aggregate properties
    }

    [Fact]
    public async Task DeleteTest_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateTestAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/test/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/test/{testData.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(1, 20)]
    public async Task GetTests_WithPaging_ShouldReturnCorrectPage(int page, int pageSize)
    {
        // Arrange
        await _databaseFixture.SeedMultipleTestsAsync(25);

        // Act
        var response = await _client.GetAsync($"/api/test?page={page}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<TestResponse>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Count.Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task CreateTest_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new CreateTestRequest(); // Empty request

        // Act
        var response = await _client.PostAsJsonAsync("/api/test", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTest_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateRequest = TestDataBuilder.CreateUpdateTestRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/test/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}