using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using ECommerce.OrderService.Api.Models;
using ECommerce.OrderService.Integration.Tests.Fixtures;
using ECommerce.OrderService.Integration.Tests.Helpers;

namespace ECommerce.OrderService.Integration.Tests.EndToEnd;

public class CustomerEndToEndTests : IClassFixture<TestApplicationFactory>, IClassFixture<DatabaseFixture>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;
    private readonly DatabaseFixture _databaseFixture;

    public CustomerEndToEndTests(TestApplicationFactory factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteCustomerWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        var createRequest = TestDataBuilder.CreateCustomerRequest();

        // Act & Assert - Create
        var createResponse = await _client.PostAsJsonAsync("/api/customer", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await createResponse.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        // Act & Assert - Get by ID
        var getResponse = await _client.GetAsync($"/api/customer/{createdId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrieved = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(createdId);

        // Act & Assert - Update
        var updateRequest = TestDataBuilder.CreateUpdateCustomerRequest();
        var updateResponse = await _client.PutAsJsonAsync($"/api/customer/{createdId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getUpdatedResponse = await _client.GetAsync($"/api/customer/{createdId}");
        var updated = await getUpdatedResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        updated.Should().NotBeNull();

        // Act & Assert - List (should include our item)
        var listResponse = await _client.GetAsync("/api/customer");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>();
        list.Should().NotBeNull();
        list!.Items.Should().Contain(x => x.Id == createdId);

        // Act & Assert - Delete
        var deleteResponse = await _client.DeleteAsync($"/api/customer/{createdId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getDeletedResponse = await _client.GetAsync($"/api/customer/{createdId}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MultipleCustomerOperations_ShouldMaintainDataConsistency()
    {
        // Arrange
        var requests = Enumerable.Range(1, 5)
            .Select(_ => TestDataBuilder.CreateCustomerRequest())
            .ToList();

        var createdIds = new List<Guid>();

        // Act - Create multiple items
        foreach (var request in requests)
        {
            var response = await _client.PostAsJsonAsync("/api/customer", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var id = await response.Content.ReadFromJsonAsync<Guid>();
            createdIds.Add(id);
        }

        // Assert - All items should be retrievable
        foreach (var id in createdIds)
        {
            var getResponse = await _client.GetAsync($"/api/customer/{id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert - List should contain all items
        var listResponse = await _client.GetAsync("/api/customer?pageSize=20");
        var list = await listResponse.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>();
        list.Should().NotBeNull();
        
        foreach (var id in createdIds)
        {
            list!.Items.Should().Contain(x => x.Id == id);
        }

        // Cleanup
        foreach (var id in createdIds)
        {
            await _client.DeleteAsync($"/api/customer/{id}");
        }
    }

    [Fact]
    public async Task CustomerOperations_WithRateLimiting_ShouldRespectLimits()
    {
        // Arrange
        var requests = Enumerable.Range(1, 100)
            .Select(_ => TestDataBuilder.CreateCustomerRequest());

        var tasks = requests.Select(async request =>
        {
            try
            {
                var response = await _client.PostAsJsonAsync("/api/customer", request);
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
    public async Task CustomerOperations_UnderLoad_ShouldMaintainPerformance()
    {
        // Arrange
        var concurrentRequests = 20;
        var requests = Enumerable.Range(1, concurrentRequests)
            .Select(_ => TestDataBuilder.CreateCustomerRequest());

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var tasks = requests.Select(async request =>
        {
            var response = await _client.PostAsJsonAsync("/api/customer", request);
            return response.StatusCode;
        });

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        results.Should().AllBe(HttpStatusCode.Created);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete within 5 seconds
    }

    [Fact]
    public async Task CustomerOperations_WithInvalidData_ShouldReturnAppropriateErrors()
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
            var response = await _client.PostAsJsonAsync("/api/customer", scenario.Request);

            // Assert
            response.StatusCode.Should().Be(scenario.ExpectedStatus);
        }
    }
}