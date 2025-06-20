using FluentValidation;

namespace ReadModelService.Application.Product.Commands.DeleteProduct;

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        // Add validation rules as needed
    }
}