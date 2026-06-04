using ClarityCare.Domain.Enums;

namespace ClarityCare.Domain.Entities;

public class IntegrationEndpoint
{
    public Guid IntegrationEndpointId { get; set; }
    public string Name { get; set; } = string.Empty;
    public IntegrationType Type { get; set; }
    public string? BaseUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastSuccessfulCall { get; set; }
    public DateTime CreatedAt { get; set; }
}
