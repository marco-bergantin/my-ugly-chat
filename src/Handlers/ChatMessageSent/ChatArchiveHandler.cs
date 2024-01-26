using Auth0Mvc.DAL;
using Auth0Mvc.Messages;
using Auth0Mvc.Services;

namespace Auth0Mvc.Handlers.ChatMessageSent;

public class ChatArchiveHandler(ChatArchiveService chatArchiveService) : IHandleMessages<ChatMessageSentEvent>
{
    private readonly ChatArchiveService _chatArchiveService = chatArchiveService;

    public async Task Handle(ChatMessageSentEvent message, IMessageHandlerContext context)
    {
        await _chatArchiveService.ArchiveMessageAsync(message.ChatId, new ChatMessage 
        {
            Id = message.Id,
            From = message.From,
            Content = message.Content,
            UtcTimestamp = message.UtcTimestamp
        });
    }
}
