using Bogus;
using ECommerce.OrderService.Api.Models;
using ECommerce.OrderService.Domain.Order;
using ECommerce.OrderService.Domain.Customer;

namespace ECommerce.OrderService.Integration.Tests.Helpers;

public static class TestDataBuilder
{
    private static readonly Faker<Order> OrderFaker = new Faker<Order>()
        .RuleFor(x => x.CustomerId, f => f.Random.Guid())
                    .RuleFor(x => x.TotalAmount, f => f.Random.Decimal(1, 1000))
                    .RuleFor(x => x.Status, f => f.Lorem.Word());

    private static readonly Faker<CreateOrderRequest> CreateOrderRequestFaker = new Faker<CreateOrderRequest>()
        .RuleFor(x => x.CustomerId, f => f.Random.Guid())
        .RuleFor(x => x.TotalAmount, f => f.Random.Decimal(1, 1000))
        .RuleFor(x => x.Status, f => f.Lorem.Word());

    private static readonly Faker<UpdateOrderRequest> UpdateOrderRequestFaker = new Faker<UpdateOrderRequest>()
        .RuleFor(x => x.CustomerId, f => f.Random.Guid())
        .RuleFor(x => x.TotalAmount, f => f.Random.Decimal(1, 1000))
        .RuleFor(x => x.Status, f => f.Lorem.Word());

    public static Order CreateOrder() => OrderFaker.Generate();

    public static List<Order> CreateMultipleOrders(int count) => OrderFaker.Generate(count);

    public static CreateOrderRequest CreateOrderRequest() => CreateOrderRequestFaker.Generate();

    public static UpdateOrderRequest CreateUpdateOrderRequest() => UpdateOrderRequestFaker.Generate();

    private static readonly Faker<Customer> CustomerFaker = new Faker<Customer>()
        .RuleFor(x => x.Email, f => f.Lorem.Word())
                    .RuleFor(x => x.Name, f => f.Lorem.Word());

    private static readonly Faker<CreateCustomerRequest> CreateCustomerRequestFaker = new Faker<CreateCustomerRequest>()
        .RuleFor(x => x.Email, f => f.Lorem.Word())
        .RuleFor(x => x.Name, f => f.Lorem.Word());

    private static readonly Faker<UpdateCustomerRequest> UpdateCustomerRequestFaker = new Faker<UpdateCustomerRequest>()
        .RuleFor(x => x.Email, f => f.Lorem.Word())
        .RuleFor(x => x.Name, f => f.Lorem.Word());

    public static Customer CreateCustomer() => CustomerFaker.Generate();

    public static List<Customer> CreateMultipleCustomers(int count) => CustomerFaker.Generate(count);

    public static CreateCustomerRequest CreateCustomerRequest() => CreateCustomerRequestFaker.Generate();

    public static UpdateCustomerRequest CreateUpdateCustomerRequest() => UpdateCustomerRequestFaker.Generate();
}