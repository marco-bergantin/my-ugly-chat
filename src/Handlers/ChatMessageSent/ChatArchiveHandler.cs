using MyUglyChat.DAL;
using MyUglyChat.Messages;
using MyUglyChat.Services;

namespace MyUglyChat.Handlers.ChatMessageSent;

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
