using System.ComponentModel.DataAnnotations;

namespace TestService.Infrastructure.Messaging;

public class OutboxEvent
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(255)]
    public string EventType { get; set; } = string.Empty;
    
    [Required]
    public string EventData { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ProcessedAt { get; set; }
    
    public bool IsProcessed { get; set; } = false;
    
    public int RetryCount { get; set; } = 0;
    
    public string? ErrorMessage { get; set; }
    
    [MaxLength(100)]
    public string? CorrelationId { get; set; }
}