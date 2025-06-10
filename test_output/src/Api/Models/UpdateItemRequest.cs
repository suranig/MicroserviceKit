using System.ComponentModel.DataAnnotations;

namespace Company.TestService.Api.Models;

/// <summary>
/// Request model for updating Item
/// </summary>
public class UpdateItemRequest
{
public string Title { get; set; }
    public bool IsCompleted { get; set; }
}