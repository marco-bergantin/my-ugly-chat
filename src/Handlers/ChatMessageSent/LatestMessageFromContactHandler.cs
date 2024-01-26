using MyUglyChat.Messages;
using MyUglyChat.Services;

namespace MyUglyChat.Handlers.ChatMessageSent;

public class LatestMessageFromContactHandler(ContactsService contactsService) : IHandleMessages<ChatMessageSentEvent>
{
    private readonly ContactsService _contactsService = contactsService;

    public async Task Handle(ChatMessageSentEvent message, IMessageHandlerContext context)
    {
        await _contactsService.UpdateLatestMessageTimestampAsync(message.To, message.From, message.UtcTimestamp);
    }
}
