using Auth0Mvc.Messages;

namespace Auth0Mvc.Handlers.RecentChatSelected;

public class ConsoleLoggerHandler : IHandleMessages<RecentChatSelectedEvent>
{
    public Task Handle(RecentChatSelectedEvent message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Message RecentChatSelectedEvent {message.Id} received at {DateTime.UtcNow}");
        return Task.CompletedTask;
    }
}
