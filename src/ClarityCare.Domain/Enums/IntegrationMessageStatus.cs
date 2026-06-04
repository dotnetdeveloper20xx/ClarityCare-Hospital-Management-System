namespace ClarityCare.Domain.Enums;

public enum IntegrationMessageStatus
{
    Queued,
    Sent,
    Delivered,
    Failed,
    DeadLettered
}
