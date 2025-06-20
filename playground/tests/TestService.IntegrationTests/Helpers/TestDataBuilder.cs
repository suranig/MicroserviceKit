using Bogus;
using TestService.Api.Models;
using TestService.Domain.Product;

namespace TestService.Integration.Tests.Helpers;

public static class TestDataBuilder
{
    private static readonly Faker<Product> ProductFaker = new Faker<Product>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
                    .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
                    .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

    private static readonly Faker<CreateProductRequest> CreateProductRequestFaker = new Faker<CreateProductRequest>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
        .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

    private static readonly Faker<UpdateProductRequest> UpdateProductRequestFaker = new Faker<UpdateProductRequest>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
        .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

    public static Product CreateProduct() => ProductFaker.Generate();

    public static List<Product> CreateMultipleProducts(int count) => ProductFaker.Generate(count);

    public static CreateProductRequest CreateProductRequest() => CreateProductRequestFaker.Generate();

    public static UpdateProductRequest CreateUpdateProductRequest() => UpdateProductRequestFaker.Generate();
}