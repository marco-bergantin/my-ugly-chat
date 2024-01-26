using MyUglyChat.DAL;
using MyUglyChat.Messages;
using MyUglyChat.Services;

namespace MyUglyChat.Handlers.ChatMessageSent;

public class RecentMessagesHandler(RecentChatsService recentChatService) : IHandleMessages<ChatMessageSentEvent>
{
    private readonly RecentChatsService _recentChatService = recentChatService;

    public async Task Handle(ChatMessageSentEvent message, IMessageHandlerContext context)
    {
        await _recentChatService.StoreRecentChatMessageAsync(message.To, message.From, new ChatMessage
        {
            Id = message.Id,
            From = message.From,
            Content = message.Content,
            UtcTimestamp = message.UtcTimestamp
        });
    }
}
