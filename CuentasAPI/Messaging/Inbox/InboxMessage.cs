namespace CuentasAPI.Messaging.Inbox;

public sealed class InboxMessage
{
    public Guid EventId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; }
}