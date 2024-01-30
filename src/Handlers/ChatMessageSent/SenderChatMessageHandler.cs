using MyUglyChat.Hubs;
using MyUglyChat.Messages;
using MyUglyChat.Models;
using MyUglyChat.Services;
using Microsoft.AspNetCore.SignalR;

namespace MyUglyChat.Handlers.ChatMessageSent;

public class SenderChatMessageHandler(IHubContext<ChatHub> hubContext, UserConnectionService userConnectionService) :
    IHandleMessages<ChatMessageSentEvent>
{
    private readonly IHubContext<ChatHub> _hubContext = hubContext;
    private readonly UserConnectionService _userConnectionService = userConnectionService;

    // handles the case where the sender has multiple sessions open, to keep the chat UI in sync
    public async Task Handle(ChatMessageSentEvent message, IMessageHandlerContext context)
    {
        var recipientConnectionIds = await _userConnectionService.GetConnectionIdsAsync(message.From);
        if (recipientConnectionIds is null || recipientConnectionIds.Length <= 1)
        {
            // if no connection found for user or not more than 1, no need to relay the message
            return;
        }

        var otherSenderConnections = recipientConnectionIds.Except(new[] { message.ConnectionId });

        foreach (var connectionId in otherSenderConnections)
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
                Direction = MessageDirection.Sent,
                Timestamp = (new DateTimeOffset(message.UtcTimestamp)).ToUnixTimeMilliseconds()
            }, context.CancellationToken);
        }
    }
}
