using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using ECommerce.OrderService.Api.Models;
using ECommerce.OrderService.Integration.Tests.Fixtures;
using ECommerce.OrderService.Integration.Tests.Helpers;

namespace ECommerce.OrderService.Integration.Tests.Api;

public class CustomerApiIntegrationTests : IClassFixture<TestApplicationFactory>, IClassFixture<DatabaseFixture>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;
    private readonly DatabaseFixture _databaseFixture;

    public CustomerApiIntegrationTests(TestApplicationFactory factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_ShouldReturnPagedResults()
    {
        // Arrange
        await _databaseFixture.SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/customer");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCustomerById_WithValidId_ShouldReturnCustomer()
    {
        // Arrange
        var testData = await _databaseFixture.CreateCustomerAsync();

        // Act
        var response = await _client.GetAsync($"/api/customer/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task GetCustomerById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customer/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldCreateAndReturnId()
    {
        // Arrange
        var testData = TestDataBuilder.CreateCustomerRequest();
        var request = new CreateCustomerRequest
        {
            Email = testData.Email,
                Name = testData.Name
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customer", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        // Verify creation
        var getResponse = await _client.GetAsync($"/api/customer/{createdId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateCustomer_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateCustomerAsync();
        var updateRequest = TestDataBuilder.CreateUpdateCustomerRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customer/{testData.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/customer/{testData.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<CustomerResponse>();
        updated.Should().NotBeNull();
        // Add specific property assertions based on aggregate properties
    }

    [Fact]
    public async Task DeleteCustomer_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateCustomerAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/customer/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/customer/{testData.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(1, 20)]
    public async Task GetCustomers_WithPaging_ShouldReturnCorrectPage(int page, int pageSize)
    {
        // Arrange
        await _databaseFixture.SeedMultipleCustomersAsync(25);

        // Act
        var response = await _client.GetAsync($"/api/customer?page={page}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<CustomerResponse>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Count.Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task CreateCustomer_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new CreateCustomerRequest(); // Empty request

        // Act
        var response = await _client.PostAsJsonAsync("/api/customer", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCustomer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateRequest = TestDataBuilder.CreateUpdateCustomerRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customer/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}