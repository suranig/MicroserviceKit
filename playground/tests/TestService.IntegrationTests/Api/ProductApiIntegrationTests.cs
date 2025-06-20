using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using TestService.Api.Models;
using TestService.Integration.Tests.Fixtures;
using TestService.Integration.Tests.Helpers;

namespace TestService.Integration.Tests.Api;

public class ProductApiIntegrationTests : IClassFixture<TestApplicationFactory>, IClassFixture<DatabaseFixture>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;
    private readonly DatabaseFixture _databaseFixture;

    public ProductApiIntegrationTests(TestApplicationFactory factory, DatabaseFixture databaseFixture)
    {
        _factory = factory;
        _databaseFixture = databaseFixture;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnPagedResults()
    {
        // Arrange
        await _databaseFixture.SeedTestDataAsync();

        // Act
        var response = await _client.GetAsync("/api/product");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
        result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var testData = await _databaseFixture.CreateProductAsync();

        // Act
        var response = await _client.GetAsync($"/api/product/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ProductResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/product/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldCreateAndReturnId()
    {
        // Arrange
        var testData = TestDataBuilder.CreateProductRequest();
        var request = new CreateProductRequest
        {
            Id = testData.Id,
                CreatedAt = testData.CreatedAt,
                UpdatedAt = testData.UpdatedAt
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/product", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>();
        createdId.Should().NotBeEmpty();

        // Verify creation
        var getResponse = await _client.GetAsync($"/api/product/{createdId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldUpdateSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateProductAsync();
        var updateRequest = TestDataBuilder.CreateUpdateProductRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/product/{testData.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify update
        var getResponse = await _client.GetAsync($"/api/product/{testData.Id}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ProductResponse>();
        updated.Should().NotBeNull();
        // Add specific property assertions based on aggregate properties
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var testData = await _databaseFixture.CreateProductAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/product/{testData.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await _client.GetAsync($"/api/product/{testData.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(1, 20)]
    public async Task GetProducts_WithPaging_ShouldReturnCorrectPage(int page, int pageSize)
    {
        // Arrange
        await _databaseFixture.SeedMultipleProductsAsync(25);

        // Act
        var response = await _client.GetAsync($"/api/product?page={page}&pageSize={pageSize}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Items.Count.Should().BeLessOrEqualTo(pageSize);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new CreateProductRequest(); // Empty request

        // Act
        var response = await _client.PostAsJsonAsync("/api/product", invalidRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        var updateRequest = TestDataBuilder.CreateUpdateProductRequest();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/product/{invalidId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}