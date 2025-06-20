using Bogus;
using SimpleService.Api.Models;
using SimpleService.Domain.User;

namespace SimpleService.Integration.Tests.Helpers;

public static class TestDataBuilder
{
    private static readonly Faker<User> UserFaker = new Faker<User>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
                    .RuleFor(x => x.Name, f => f.Lorem.Word())
                    .RuleFor(x => x.Description, f => f.Lorem.Word());

    private static readonly Faker<CreateUserRequest> CreateUserRequestFaker = new Faker<CreateUserRequest>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Name, f => f.Lorem.Word())
        .RuleFor(x => x.Description, f => f.Lorem.Word());

    private static readonly Faker<UpdateUserRequest> UpdateUserRequestFaker = new Faker<UpdateUserRequest>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Name, f => f.Lorem.Word())
        .RuleFor(x => x.Description, f => f.Lorem.Word());

    public static User CreateUser() => UserFaker.Generate();

    public static List<User> CreateMultipleUsers(int count) => UserFaker.Generate(count);

    public static CreateUserRequest CreateUserRequest() => CreateUserRequestFaker.Generate();

    public static UpdateUserRequest CreateUpdateUserRequest() => UpdateUserRequestFaker.Generate();
}