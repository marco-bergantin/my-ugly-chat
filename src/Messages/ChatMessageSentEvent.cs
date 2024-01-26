namespace MyUglyChat.Messages;

public class ChatMessageSentEvent : IEvent
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public required string From { get; set; }
    public required string To { get; set; }
    public required string ChatId { get; set; }
    public required string ConnectionId { get; set; }
    public required DateTime UtcTimestamp { get; set; }
}
