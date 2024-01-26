using Auth0Mvc.DAL;
using Auth0Mvc.Messages;
using Auth0Mvc.Services;

namespace Auth0Mvc.Handlers.ChatMessageSent;

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
