using MyUglyChat.Messages;
using MyUglyChat.Services;

namespace MyUglyChat.Handlers.ChatMessageSent;

public class ConsoleLoggerHandler(MetricsService metricsService) : IHandleMessages<ChatMessageSentEvent>
{
    private readonly MetricsService _metricsService = metricsService;

    public Task Handle(ChatMessageSentEvent message, IMessageHandlerContext context)
    {
        _metricsService.AddMessageSent();
        Console.WriteLine($"Message ChatMessageSentEvent {message.Id} received at {DateTime.UtcNow}");
        return Task.CompletedTask;
    }
}
