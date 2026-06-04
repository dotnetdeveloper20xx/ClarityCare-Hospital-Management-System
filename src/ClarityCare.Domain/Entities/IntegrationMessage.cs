using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class IntegrationMessage
{
    public Guid MessageId { get; set; }
    public string? CorrelationId { get; set; }
    public Guid EndpointId { get; set; }
    public string? Payload { get; set; }
    public IntegrationMessageStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }

    public IntegrationEndpoint Endpoint { get; set; } = null!;
}
