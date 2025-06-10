using Bogus;
using Company.TestService.Api.Models;
using Company.TestService.Domain.Item;

namespace Company.TestService.Integration.Tests.Helpers;

public static class TestDataBuilder
{
    private static readonly Faker<Item> ItemFaker = new Faker<Item>()
        .RuleFor(x => x.Title, f => f.Lorem.Word())
                    .RuleFor(x => x.IsCompleted, f => f.Random.Bool());

    private static readonly Faker<CreateItemRequest> CreateItemRequestFaker = new Faker<CreateItemRequest>()
        .RuleFor(x => x.Title, f => f.Lorem.Word())
        .RuleFor(x => x.IsCompleted, f => f.Random.Bool());

    private static readonly Faker<UpdateItemRequest> UpdateItemRequestFaker = new Faker<UpdateItemRequest>()
        .RuleFor(x => x.Title, f => f.Lorem.Word())
        .RuleFor(x => x.IsCompleted, f => f.Random.Bool());

    public static Item CreateItem() => ItemFaker.Generate();

    public static List<Item> CreateMultipleItems(int count) => ItemFaker.Generate(count);

    public static CreateItemRequest CreateItemRequest() => CreateItemRequestFaker.Generate();

    public static UpdateItemRequest CreateUpdateItemRequest() => UpdateItemRequestFaker.Generate();
}