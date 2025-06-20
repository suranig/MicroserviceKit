using Bogus;
using TestService.Api.Models;
using TestService.Domain.Test;

namespace TestService.Integration.Tests.Helpers;

public static class TestDataBuilder
{
    private static readonly Faker<Test> TestFaker = new Faker<Test>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
                    .RuleFor(x => x.Name, f => f.Lorem.Word())
                    .RuleFor(x => x.Description, f => f.Lorem.Word())
                    .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
                    .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

    private static readonly Faker<CreateTestRequest> CreateTestRequestFaker = new Faker<CreateTestRequest>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Name, f => f.Lorem.Word())
        .RuleFor(x => x.Description, f => f.Lorem.Word())
        .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
        .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

    private static readonly Faker<UpdateTestRequest> UpdateTestRequestFaker = new Faker<UpdateTestRequest>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.Name, f => f.Lorem.Word())
        .RuleFor(x => x.Description, f => f.Lorem.Word())
        .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
        .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

    public static Test CreateTest() => TestFaker.Generate();

    public static List<Test> CreateMultipleTests(int count) => TestFaker.Generate(count);

    public static CreateTestRequest CreateTestRequest() => CreateTestRequestFaker.Generate();

    public static UpdateTestRequest CreateUpdateTestRequest() => UpdateTestRequestFaker.Generate();
}