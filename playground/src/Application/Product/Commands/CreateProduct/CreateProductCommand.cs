namespace TestService.Application.Product.Commands.CreateProduct;

public record CreateProductCommand(Guid id, DateTime createdAt, DateTime updatedAt);