namespace ClarityCare.Domain.Entities;

public class AIInteraction
{
    public Guid InteractionId { get; set; }
    public Guid UserId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string? Model { get; set; }
    public int TokensUsed { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}
