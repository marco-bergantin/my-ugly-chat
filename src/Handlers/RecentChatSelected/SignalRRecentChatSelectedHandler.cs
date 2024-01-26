using MyUglyChat.Hubs;
using MyUglyChat.Messages;
using MyUglyChat.Models;
using MyUglyChat.Services;
using Microsoft.AspNetCore.SignalR;

namespace MyUglyChat.Handlers.RecentChatSelected;

public class SignalRRecentChatSelectedHandler(IHubContext<ChatHub> hubContext, RecentChatsService recentChatsService) : IHandleMessages<RecentChatSelectedEvent>
{
    private readonly IHubContext<ChatHub> _hubContext = hubContext;
    private readonly RecentChatsService _recentChatsService = recentChatsService;

    public async Task Handle(RecentChatSelectedEvent message, IMessageHandlerContext context)
    {
        var recipientConnectionId = message.ConnectionId;
        if (string.IsNullOrWhiteSpace(recipientConnectionId))
        {
            return; // not for this instance
        }

        var hubClient = _hubContext.Clients.Client(recipientConnectionId);
        if (hubClient is null)
        {
            // log warning: we think we have this connection for this user, but it actually fell from the hub
            return;
        }

        var recentChat = await _recentChatsService.GetRecentChatAsync(message.ChatId);
        if (recentChat is null)
        {
            return; // no recent chat found, nothing to do
        }

        foreach (var chatMessage in recentChat.Messages)
        {
            await hubClient.SendAsync("ReceiveMessage", new ChatMessageViewModel
            {
                User = chatMessage.From,
                Content = chatMessage.Content,
                ChatId = message.ChatId,
                Direction = chatMessage.From == message.SelectedBy ? MessageDirection.Sent : MessageDirection.Received,
                Timestamp = (new DateTimeOffset(chatMessage.UtcTimestamp)).ToUnixTimeMilliseconds(),
                FromArchive = true // not exactly from archive but in reverse chronological order (so they need to be added on top in the UI)
            },
            context.CancellationToken);
        }
    }
}
