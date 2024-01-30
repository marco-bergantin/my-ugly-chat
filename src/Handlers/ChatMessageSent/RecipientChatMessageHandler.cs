using MyUglyChat.Hubs;
using MyUglyChat.Messages;
using MyUglyChat.Models;
using MyUglyChat.Services;
using Microsoft.AspNetCore.SignalR;

namespace MyUglyChat.Handlers.ChatMessageSent;

public class RecipientChatMessageHandler(IHubContext<ChatHub> hubContext, UserConnectionService userConnectionService) :
    IHandleMessages<ChatMessageSentEvent>
{
    private readonly IHubContext<ChatHub> _hubContext = hubContext;
    private readonly UserConnectionService _userConnectionService = userConnectionService;

    public async Task Handle(ChatMessageSentEvent message, IMessageHandlerContext context)
    {
        var recipientConnectionIds = await _userConnectionService.GetConnectionIdsAsync(message.To);
        if (recipientConnectionIds is null || recipientConnectionIds.Length == 0)
        {
            // log warning? no record of a connection for user message.To
            return;
        }

        foreach (var connectionId in recipientConnectionIds)
        {
            var hubClient = _hubContext.Clients.Client(connectionId);
            if (hubClient is null)
            {
                // log warning? there's a record of a connection, which we could not found (possible timing issue)
                return;
            }

            // idempotency handled in front end in this case, see isAlreadyAdded in chat.js
            await hubClient.SendAsync("ReceiveMessage", new ChatMessageViewModel
            {
                MessageId = message.Id,
                User = message.From,
                ChatId = message.ChatId,
                Content = message.Content,
                Direction = MessageDirection.Received,
                Timestamp = (new DateTimeOffset(message.UtcTimestamp)).ToUnixTimeMilliseconds()
            }, context.CancellationToken);
        }
    }
}
