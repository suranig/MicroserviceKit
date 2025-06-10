using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Company.TestService.Domain.Item;
using Company.TestService.Infrastructure.Persistence;
using Company.TestService.Infrastructure.Repositories;
using Company.TestService.Integration.Tests.Fixtures;
using Company.TestService.Integration.Tests.Helpers;

namespace Company.TestService.Integration.Tests.Database;

public class ItemDatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public ItemDatabaseIntegrationTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Repository_Add_ShouldPersistItemToDatabase()
    {
        // Arrange
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ItemRepository(context);
        var item = TestDataBuilder.CreateItem();

        // Act
        await repository.AddAsync(item);
        await context.SaveChangesAsync();

        // Assert
        var savedItem = await context.Items.FirstOrDefaultAsync(x => x.Id == item.Id);
        savedItem.Should().NotBeNull();
        savedItem!.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task Repository_GetById_ShouldRetrieveItemFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateItemAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ItemRepository(context);

        // Act
        var result = await repository.GetByIdAsync(testData.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task Repository_Update_ShouldModifyItemInDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateItemAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ItemRepository(context);

        // Act
        var item = await repository.GetByIdAsync(testData.Id);
        item.Should().NotBeNull();
        
        // Modify entity (add specific property modifications based on aggregate)
        repository.Update(item!);
        await context.SaveChangesAsync();

        // Assert
        var updatedItem = await repository.GetByIdAsync(testData.Id);
        updatedItem.Should().NotBeNull();
        // Add specific assertions for modified properties
    }

    [Fact]
    public async Task Repository_Delete_ShouldRemoveItemFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateItemAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ItemRepository(context);

        // Act
        var item = await repository.GetByIdAsync(testData.Id);
        item.Should().NotBeNull();
        
        repository.Delete(item!);
        await context.SaveChangesAsync();

        // Assert
        var deletedItem = await repository.GetByIdAsync(testData.Id);
        deletedItem.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetAll_ShouldReturnAllItems()
    {
        // Arrange
        await _databaseFixture.SeedMultipleItemsAsync(5);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ItemRepository(context);

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
        await _databaseFixture.SeedMultipleItemsAsync(20);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new ItemRepository(context);

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
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Items'");
        tableExists.First().Should().Be(1);
    }

    [Fact]
    public async Task DbContext_ConcurrencyHandling_ShouldHandleOptimisticLocking()
    {
        // Arrange
        var testData = await _databaseFixture.CreateItemAsync();
        
        using var context1 = _databaseFixture.CreateDbContext();
        using var context2 = _databaseFixture.CreateDbContext();
        
        var repository1 = new ItemRepository(context1);
        var repository2 = new ItemRepository(context2);

        // Act
        var item1 = await repository1.GetByIdAsync(testData.Id);
        var item2 = await repository2.GetByIdAsync(testData.Id);

        item1.Should().NotBeNull();
        item2.Should().NotBeNull();

        // Modify both entities
        repository1.Update(item1!);
        repository2.Update(item2!);

        // Save first context
        await context1.SaveChangesAsync();

        // Assert
        // Second context should throw concurrency exception
        var act = async () => await context2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}