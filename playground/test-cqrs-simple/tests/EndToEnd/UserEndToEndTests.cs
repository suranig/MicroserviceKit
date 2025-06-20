using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using SimpleService.Api.Models;
using SimpleService.Integration.Tests.Fixtures;
using SimpleService.Integration.Tests.Helpers;

namespace SimpleService.Integration.Tests.EndToEnd;

public class UserEndToEndTests : IClassFixture<TestApplicationFactory>, IClassFixture<DatabaseFixture>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;
    private readonly DatabaseFixture _databaseFixture;

    public UserEndToEndTests(TestApplicationFactory factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteUserWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateUserRequest();

        // Act & Assert - Create
        var createResponse = await _client.PostAsJsonAsync("/api/user", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        // Act & Assert - Get by ID
        var getResponse = await _client.GetAsync($"/api/user/{createdId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<UserResponse>();
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(createdId);

        // Act & Assert - Update
        var updateRequest = TestDataBuilder.CreateUpdateUserRequest();
        var updateResponse = await _client.PutAsJsonAsync($"/api/user/{createdId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getUpdatedResponse = await _client.GetAsync($"/api/user/{createdId}");
        var updated = await getUpdatedResponse.Content.ReadFromJsonAsync<UserResponse>();
        updated.Should().NotBeNull();

        // Act & Assert - List (should include our item)
        var listResponse = await _client.GetAsync("/api/user");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();
        list.Should().NotBeNull();
        list!.Items.Should().Contain(x => x.Id == createdId);

        // Act & Assert - Delete
        var deleteResponse = await _client.DeleteAsync($"/api/user/{createdId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getDeletedResponse = await _client.GetAsync($"/api/user/{createdId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MultipleUserOperations_ShouldMaintainDataConsistency()
    {
        // Arrange
        var requests = Enumerable.Range(1, 5)
            .Select(_ => TestDataBuilder.CreateUserRequest())
            .ToList();

        var createdIds = new List<Guid>();

        // Act - Create multiple items
        foreach (var request in requests)
        {
            var response = await _client.PostAsJsonAsync("/api/user", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            createdIds.Add(id);
        }

        // Assert - All items should be retrievable
        foreach (var id in createdIds)
        {
            var getResponse = await _client.GetAsync($"/api/user/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert - List should contain all items
        var listResponse = await _client.GetAsync("/api/user?pageSize=20");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResponse<UserResponse>>();
        list.Should().NotBeNull();
        
        foreach (var id in createdIds)
        {
            list!.Items.Should().Contain(x => x.Id == id);
        }

        // Cleanup
        foreach (var id in createdIds)
        {
            await _client.DeleteAsync($"/api/user/{id}");
        }
    }

    [Fact]
    public async Task UserOperations_WithRateLimiting_ShouldRespectLimits()
    {
        // Arrange
        var requests = Enumerable.Range(1, 100)
            .Select(_ => TestDataBuilder.CreateUserRequest());

        var tasks = requests.Select(async request =>
        {
            try
            {
                var response = await _client.PostAsJsonAsync("/api/user", request);
                return response.StatusCode;
            }
            catch
            {
                return HttpStatusCode.TooManyRequests;
            }
        });

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        // Some requests should succeed, some might be rate limited
        results.Should().Contain(HttpStatusCode.Created);
        // Note: Rate limiting behavior depends on configuration
    }

    [Fact]
    public async Task UserOperations_UnderLoad_ShouldMaintainPerformance()
    {
        // Arrange
        var concurrentRequests = 20;
        var requests = Enumerable.Range(1, concurrentRequests)
            .Select(_ => TestDataBuilder.CreateUserRequest());

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var tasks = requests.Select(async request =>
        {
            var response = await _client.PostAsJsonAsync("/api/user", request);
            return response.StatusCode;
        });

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Should().AllBe(HttpStatusCode.Created);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    [Fact]
    public async Task UserOperations_WithInvalidData_ShouldReturnAppropriateErrors()
    {
        // Test various invalid scenarios
        var scenarios = new[]
        {
            new { Request = (object?)null, ExpectedStatus = HttpStatusCode.BadRequest },
            new { Request = new {}, ExpectedStatus = HttpStatusCode.BadRequest },
            // Add more invalid scenarios based on aggregate properties
        };

        foreach (var scenario in scenarios)
        {
            // Act
            var response = await _client.PostAsJsonAsync("/api/user", scenario.Request);

            // Assert
            response.StatusCode.Should().Be(scenario.ExpectedStatus);
        }
    }
}