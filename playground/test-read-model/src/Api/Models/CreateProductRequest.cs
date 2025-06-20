using System.ComponentModel.DataAnnotations;

namespace ReadModelService.Api.Models;

/// <summary>
/// Request model for creating Product
/// </summary>
public class CreateProductRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}