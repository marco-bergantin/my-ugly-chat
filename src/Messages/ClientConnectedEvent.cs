namespace Auth0Mvc.Messages;

public class ClientConnectedEvent : IEvent
{
    public required string UserId { get; init; }
    public required string ConnectionId { get; init; }
    public required string Host { get; init; }
}
