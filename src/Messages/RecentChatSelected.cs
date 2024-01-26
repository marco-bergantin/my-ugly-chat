namespace MyUglyChat.Messages;

public class RecentChatSelectedEvent : IEvent
{
    public required Guid Id { get; init; }
    public required string ChatId { get; init; }
    // only need to know to which user we need to push the recent chat messages to
    public required string SelectedBy { get; init; }
    public required string ConnectionId { get; init; }
}
