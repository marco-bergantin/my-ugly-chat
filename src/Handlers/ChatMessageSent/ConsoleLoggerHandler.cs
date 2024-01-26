using Auth0Mvc.Messages;

namespace Auth0Mvc.Handlers.ChatMessageSent;

public class ConsoleLoggerHandler : IHandleMessages<ChatMessageSentEvent>
{
    public Task Handle(ChatMessageSentEvent message, IMessageHandlerContext context)
    {
        Console.WriteLine($"Message ChatMessageSentEvent {message.Id} received at {DateTime.UtcNow}");
        return Task.CompletedTask;
    }
}
