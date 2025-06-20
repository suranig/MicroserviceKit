using System.ComponentModel.DataAnnotations;

namespace TestService.Api.Models;

/// <summary>
/// Request model for updating Product
/// </summary>
public class UpdateProductRequest
{
public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}