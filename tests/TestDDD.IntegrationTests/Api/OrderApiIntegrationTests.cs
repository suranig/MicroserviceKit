using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using ECommerce.OrderService.Api.Models;
using ECommerce.OrderService.Integration.Tests.Fixtures;
using ECommerce.OrderService.Integration.Tests.Helpers;

namespace ECommerce.OrderService.Integration.Tests.Api;

public class OrderApiIntegrationTests : IClassFixture<TestApplicationFactory>, IClassFixture<DatabaseFixture>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;
    private readonly DatabaseFixture _databaseFixture;

    public OrderApiIntegrationTests(TestApplicationFactory factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrders_ShouldReturnPagedResults()
    {
        // Arrange
        await _databaseFixture.SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/order");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<OrderResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetOrderById_WithValidId_ShouldReturnOrder()
    {
        // Arrange
        var testData = await _databaseFixture.CreateOrderAsync();

        // Act
        var response = await _client.GetAsync($"/api/order/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrderResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task GetOrderById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/order/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateOrder_WithValidData_ShouldCreateAndReturnId()
    {
        // Arrange
        var testData = TestDataBuilder.CreateOrderRequest();
        var request = new CreateOrderRequest
        {
            CustomerId = testData.CustomerId,
                TotalAmount = testData.TotalAmount,
                Status = testData.Status
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/order", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        // Verify creation
        var getResponse = await _client.GetAsync($"/api/order/{createdId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateOrder_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateOrderAsync();
        var updateRequest = TestDataBuilder.CreateUpdateOrderRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/order/{testData.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/order/{testData.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<OrderResponse>();
        updated.Should().NotBeNull();
        // Add specific property assertions based on aggregate properties
    }

    [Fact]
    public async Task DeleteOrder_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateOrderAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/order/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/order/{testData.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(1, 20)]
    public async Task GetOrders_WithPaging_ShouldReturnCorrectPage(int page, int pageSize)
    {
        // Arrange
        await _databaseFixture.SeedMultipleOrdersAsync(25);

        // Act
        var response = await _client.GetAsync($"/api/order?page={page}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<OrderResponse>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Count.Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task CreateOrder_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new CreateOrderRequest(); // Empty request

        // Act
        var response = await _client.PostAsJsonAsync("/api/order", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateOrder_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateRequest = TestDataBuilder.CreateUpdateOrderRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/order/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}