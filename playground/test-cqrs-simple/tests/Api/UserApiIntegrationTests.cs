using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using SimpleService.Api.Models;
using SimpleService.Integration.Tests.Fixtures;
using SimpleService.Integration.Tests.Helpers;

namespace SimpleService.Integration.Tests.Api;

public class UserApiIntegrationTests : IClassFixture<TestApplicationFactory>, IClassFixture<DatabaseFixture>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;
    private readonly DatabaseFixture _databaseFixture;

    public UserApiIntegrationTests(TestApplicationFactory factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_ShouldReturnPagedResults()
    {
        // Arrange
        await _databaseFixture.SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetUserById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var testData = await _databaseFixture.CreateUserAsync();

        // Act
        var response = await _client.GetAsync($"/api/user/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task GetUserById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/user/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldCreateAndReturnId()
    {
        // Arrange
        var testData = TestDataBuilder.CreateUserRequest();
        var request = new CreateUserRequest
        {
            Id = testData.Id,
                Name = testData.Name,
                Description = testData.Description
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/user", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        // Verify creation
        var getResponse = await _client.GetAsync($"/api/user/{createdId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateUserAsync();
        var updateRequest = TestDataBuilder.CreateUpdateUserRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{testData.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/user/{testData.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<UserResponse>();
        updated.Should().NotBeNull();
        // Add specific property assertions based on aggregate properties
    }

    [Fact]
    public async Task DeleteUser_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateUserAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/user/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/user/{testData.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(1, 20)]
    public async Task GetUsers_WithPaging_ShouldReturnCorrectPage(int page, int pageSize)
    {
        // Arrange
        await _databaseFixture.SeedMultipleUsersAsync(25);

        // Act
        var response = await _client.GetAsync($"/api/user?page={page}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Count.Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new CreateUserRequest(); // Empty request

        // Act
        var response = await _client.PostAsJsonAsync("/api/user", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateRequest = TestDataBuilder.CreateUpdateUserRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/user/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}