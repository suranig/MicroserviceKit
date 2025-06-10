using System.ComponentModel.DataAnnotations;

namespace Company.TestService.Api.Models;

/// <summary>
/// Request model for creating Item
/// </summary>
public class CreateItemRequest
{
public string Title { get; set; }
    public bool IsCompleted { get; set; }
}