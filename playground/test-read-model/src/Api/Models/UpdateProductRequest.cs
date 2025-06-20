using System.ComponentModel.DataAnnotations;

namespace ReadModelService.Api.Models;

/// <summary>
/// Request model for updating Product
/// </summary>
public class UpdateProductRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}