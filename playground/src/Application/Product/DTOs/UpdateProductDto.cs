namespace TestService.Application.Product.DTOs;

public class UpdateProductDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}