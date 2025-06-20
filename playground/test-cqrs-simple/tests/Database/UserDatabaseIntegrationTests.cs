using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SimpleService.Domain.User;
using SimpleService.Infrastructure.Persistence;
using SimpleService.Infrastructure.Repositories;
using SimpleService.Integration.Tests.Fixtures;
using SimpleService.Integration.Tests.Helpers;

namespace SimpleService.Integration.Tests.Database;

public class UserDatabaseIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public UserDatabaseIntegrationTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Repository_Add_ShouldPersistUserToDatabase()
    {
        // Arrange
        using var context = _databaseFixture.CreateDbContext();
        var repository = new UserRepository(context);
        var user = TestDataBuilder.CreateUser();

        // Act
        await repository.AddAsync(user);
        await context.SaveChangesAsync();

        // Assert
        var savedUser = await context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Repository_GetById_ShouldRetrieveUserFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateUserAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new UserRepository(context);

        // Act
        var result = await repository.GetByIdAsync(testData.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(testData.Id);
    }

    [Fact]
    public async Task Repository_Update_ShouldModifyUserInDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateUserAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new UserRepository(context);

        // Act
        var user = await repository.GetByIdAsync(testData.Id);
        user.Should().NotBeNull();
        
        // Modify entity (add specific property modifications based on aggregate)
        repository.Update(user!);
        await context.SaveChangesAsync();

        // Assert
        var updatedUser = await repository.GetByIdAsync(testData.Id);
        updatedUser.Should().NotBeNull();
        // Add specific assertions for modified properties
    }

    [Fact]
    public async Task Repository_Delete_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var testData = await _databaseFixture.CreateUserAsync();
        using var context = _databaseFixture.CreateDbContext();
        var repository = new UserRepository(context);

        // Act
        var user = await repository.GetByIdAsync(testData.Id);
        user.Should().NotBeNull();
        
        repository.Delete(user!);
        await context.SaveChangesAsync();

        // Assert
        var deletedUser = await repository.GetByIdAsync(testData.Id);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task Repository_GetAll_ShouldReturnAllUsers()
    {
        // Arrange
        await _databaseFixture.SeedMultipleUsersAsync(5);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new UserRepository(context);

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
        await _databaseFixture.SeedMultipleUsersAsync(20);
        using var context = _databaseFixture.CreateDbContext();
        var repository = new UserRepository(context);

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
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'Users'");
        tableExists.First().Should().Be(1);
    }

    [Fact]
    public async Task DbContext_ConcurrencyHandling_ShouldHandleOptimisticLocking()
    {
        // Arrange
        var testData = await _databaseFixture.CreateUserAsync();
        
        using var context1 = _databaseFixture.CreateDbContext();
        using var context2 = _databaseFixture.CreateDbContext();
        
        var repository1 = new UserRepository(context1);
        var repository2 = new UserRepository(context2);

        // Act
        var user1 = await repository1.GetByIdAsync(testData.Id);
        var user2 = await repository2.GetByIdAsync(testData.Id);

        user1.Should().NotBeNull();
        user2.Should().NotBeNull();

        // Modify both entities
        repository1.Update(user1!);
        repository2.Update(user2!);

        // Save first context
        await context1.SaveChangesAsync();

        // Assert
        // Second context should throw concurrency exception
        var act = async () => await context2.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateConcurrencyException>();
    }
}