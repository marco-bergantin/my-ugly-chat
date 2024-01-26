namespace MyUglyChat.Messages;

public class ClientDisconnectedEvent : IEvent
{
    public required string UserId { get; init; }
    public required string ConnectionId { get; init; }
    public required string Host { get; init; }
}
