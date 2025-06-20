namespace ReadModelService.Application.Product.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, string Description);