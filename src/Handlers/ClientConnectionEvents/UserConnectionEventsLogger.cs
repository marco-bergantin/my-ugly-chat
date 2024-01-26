using Auth0Mvc.Messages;

namespace Auth0Mvc.Handlers.ClientConnectionEvents;

public class UserConnectionEventsLogger : IHandleMessages<ClientConnectedEvent>,
    IHandleMessages<ClientDisconnectedEvent>
{
    public Task Handle(ClientConnectedEvent message, IMessageHandlerContext context)
    {
        Console.WriteLine($"User {message.UserId} connected on {message.Host} at {DateTime.UtcNow}");
        return Task.CompletedTask;
    }

    public Task Handle(ClientDisconnectedEvent message, IMessageHandlerContext context)
    {
        Console.WriteLine($"User {message.UserId} disconnected from {message.Host} at {DateTime.UtcNow}");
        return Task.CompletedTask;
    }
}
