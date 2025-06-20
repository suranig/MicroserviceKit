using System.ComponentModel.DataAnnotations;

namespace TestService.Api.Models;

/// <summary>
/// Request model for creating Product
/// </summary>
public class CreateProductRequest
{
public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}