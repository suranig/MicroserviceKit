using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestService.Domain.Product;
using TestService.Infrastructure.Persistence;
using TestService.Infrastructure.Repositories;
using TestService.Integration.Tests.Fixtures;
using TestService.Integration.Tests.Helpers;

namespace TestService.Integration.Tests.Database;

public class ProductDatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public ProductDatabaseIntegrationTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Repository_Add_ShouldPersistProductToDatabase()
    {
        // Arrange
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ProductRepository(context);
        var product = TestDataBuilder.CreateProduct();

        // Act
        await repository.AddAsync(product);
        await context.SaveChangesAsync();

        // Assert
        var savedProduct = await context.Products.FirstOrDefaultAsync(x => x.Id == product.Id);
        savedProduct.Should().NotBeNull();
        savedProduct!.Id.Should().Be(product.Id);
    }

    [Fact]
    public async Task Repository_GetById_ShouldRetrieveProductFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateProductAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ProductRepository(context);

        // Act
        var result = await repository.GetByIdAsync(testData.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task Repository_Update_ShouldModifyProductInDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateProductAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ProductRepository(context);

        // Act
        var product = await repository.GetByIdAsync(testData.Id);
        product.Should().NotBeNull();
        
        // Modify entity (add specific property modifications based on aggregate)
        repository.Update(product!);
        await context.SaveChangesAsync();

        // Assert
        var updatedProduct = await repository.GetByIdAsync(testData.Id);
        updatedProduct.Should().NotBeNull();
        // Add specific assertions for modified properties
    }

    [Fact]
    public async Task Repository_Delete_ShouldRemoveProductFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateProductAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ProductRepository(context);

        // Act
        var product = await repository.GetByIdAsync(testData.Id);
        product.Should().NotBeNull();
        
        repository.Delete(product!);
        await context.SaveChangesAsync();

        // Assert
        var deletedProduct = await repository.GetByIdAsync(testData.Id);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetAll_ShouldReturnAllProducts()
    {
        // Arrange
        await _databaseFixture.SeedMultipleProductsAsync(5);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ProductRepository(context);

        // Act
        var results = await repository.GetAllAsync();

        // Assert
        results.Should().NotBeEmpty();
        results.Count().Should().BeGreaterOrEqualTo(5);
    }

    [Fact]
    public async Task Repository_GetWithPaging_ShouldReturnPagedResults()
    {
        // Arrange
        await _databaseFixture.SeedMultipleProductsAsync(20);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ProductRepository(context);

        // Act
        var results = await repository.GetPagedAsync(1, 10);

        // Assert
        results.Should().NotBeNull();
        results.Items.Should().HaveCount(10);
        results.TotalCount.Should().BeGreaterOrEqualTo(20);
        results.Page.Should().Be(1);
        results.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task DbContext_Migrations_ShouldCreateCorrectSchema()
    {
        // Arrange & Act
        using var context = _databaseFixture.CreateDbContext();
        await context.Database.EnsureCreatedAsync();

        // Assert
        var tableExists = await context.Database.GetDbConnection().QueryAsync<int>(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Products'");
        tableExists.First().Should().Be(1);
    }

    [Fact]
    public async Task DbContext_ConcurrencyHandling_ShouldHandleOptimisticLocking()
    {
        // Arrange
        var testData = await _databaseFixture.CreateProductAsync();
        
        using var context1 = _databaseFixture.CreateDbContext();
        using var context2 = _databaseFixture.CreateDbContext();
        
        var repository1 = new ProductRepository(context1);
        var repository2 = new ProductRepository(context2);

        // Act
        var product1 = await repository1.GetByIdAsync(testData.Id);
        var product2 = await repository2.GetByIdAsync(testData.Id);

        product1.Should().NotBeNull();
        product2.Should().NotBeNull();

        // Modify both entities
        repository1.Update(product1!);
        repository2.Update(product2!);

        // Save first context
        await context1.SaveChangesAsync();

        // Assert
        // Second context should throw concurrency exception
        var act = async () => await context2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}