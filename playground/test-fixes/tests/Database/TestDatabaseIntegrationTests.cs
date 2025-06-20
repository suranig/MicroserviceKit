using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TestService.Domain.Test;
using TestService.Infrastructure.Persistence;
using TestService.Infrastructure.Repositories;
using TestService.Integration.Tests.Fixtures;
using TestService.Integration.Tests.Helpers;

namespace TestService.Integration.Tests.Database;

public class TestDatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public TestDatabaseIntegrationTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Repository_Add_ShouldPersistTestToDatabase()
    {
        // Arrange
        using var context = _databaseFixture.CreateDbContext();
        var repository = new TestRepository(context);
        var test = TestDataBuilder.CreateTest();

        // Act
        await repository.AddAsync(test);
        await context.SaveChangesAsync();

        // Assert
        var savedTest = await context.Tests.FirstOrDefaultAsync(x => x.Id == test.Id);
        savedTest.Should().NotBeNull();
        savedTest!.Id.Should().Be(test.Id);
    }

    [Fact]
    public async Task Repository_GetById_ShouldRetrieveTestFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateTestAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new TestRepository(context);

        // Act
        var result = await repository.GetByIdAsync(testData.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task Repository_Update_ShouldModifyTestInDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateTestAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new TestRepository(context);

        // Act
        var test = await repository.GetByIdAsync(testData.Id);
        test.Should().NotBeNull();
        
        // Modify entity (add specific property modifications based on aggregate)
        repository.Update(test!);
        await context.SaveChangesAsync();

        // Assert
        var updatedTest = await repository.GetByIdAsync(testData.Id);
        updatedTest.Should().NotBeNull();
        // Add specific assertions for modified properties
    }

    [Fact]
    public async Task Repository_Delete_ShouldRemoveTestFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateTestAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new TestRepository(context);

        // Act
        var test = await repository.GetByIdAsync(testData.Id);
        test.Should().NotBeNull();
        
        repository.Delete(test!);
        await context.SaveChangesAsync();

        // Assert
        var deletedTest = await repository.GetByIdAsync(testData.Id);
        deletedTest.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetAll_ShouldReturnAllTests()
    {
        // Arrange
        await _databaseFixture.SeedMultipleTestsAsync(5);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new TestRepository(context);

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
        await _databaseFixture.SeedMultipleTestsAsync(20);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new TestRepository(context);

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
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Tests'");
        tableExists.First().Should().Be(1);
    }

    [Fact]
    public async Task DbContext_ConcurrencyHandling_ShouldHandleOptimisticLocking()
    {
        // Arrange
        var testData = await _databaseFixture.CreateTestAsync();
        
        using var context1 = _databaseFixture.CreateDbContext();
        using var context2 = _databaseFixture.CreateDbContext();
        
        var repository1 = new TestRepository(context1);
        var repository2 = new TestRepository(context2);

        // Act
        var test1 = await repository1.GetByIdAsync(testData.Id);
        var test2 = await repository2.GetByIdAsync(testData.Id);

        test1.Should().NotBeNull();
        test2.Should().NotBeNull();

        // Modify both entities
        repository1.Update(test1!);
        repository2.Update(test2!);

        // Save first context
        await context1.SaveChangesAsync();

        // Assert
        // Second context should throw concurrency exception
        var act = async () => await context2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}