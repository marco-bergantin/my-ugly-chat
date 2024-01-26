using MyUglyChat.Helpers;
using MyUglyChat.Messages;
using MyUglyChat.Models;
using MyUglyChat.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace MyUglyChat.Hubs;

public class ChatHub(IMessageSession messageSession, 
    ContactsService contactsService,
    ChatArchiveService chatArchiveService) : Hub
{
    private readonly IMessageSession _messageSession = messageSession;
    private readonly ContactsService _contactsService = contactsService;
    private readonly ChatArchiveService _chatArchiveService = chatArchiveService;

    public async Task SendMessage(string chatId, string to, string message)
    {
        if (Context is null)
        {
            throw new InvalidOperationException(nameof(Context));
        }

        var senderUserId = Context.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(senderUserId))
        {
            // should never happen
            throw new UnauthorizedAccessException();
        }

        var chatMessageEvent = new ChatMessageSentEvent
        {
            Id = Guid.NewGuid(),
            Content = message,
            From = senderUserId,
            To = to,
            ChatId = chatId,
            ConnectionId = Context.ConnectionId,
            UtcTimestamp = DateTime.UtcNow
        };

        await _messageSession.Publish(chatMessageEvent);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context?.User?.GetUserId();
        if (Context != null && !string.IsNullOrWhiteSpace(userId))
        {
            await _messageSession.Publish(new ClientConnectedEvent
            { 
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                Host = GetLocalHost()
            });

            var contactList = await _contactsService.GetContactsAsync(userId);

            var contacts = new ContactsViewModel
            {
                Contacts = contactList?.Contacts?.OrderByDescending(c => c.TimestampLatestMessage)
                                                 .Select(c => new ContactViewModel { 
                                                     DisplayName = c.DisplayName,
                                                     UserId = c.UserId,
                                                     ChatId = RecentChatsService.GetChatId(userId, c.UserId),
                                                     TimestampLatestMessage = c.TimestampLatestMessage
                                                 })
                         ?? []
            };

            await Clients.Client(Context.ConnectionId).SendAsync("RefreshRecentContacts", contacts);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context?.User?.GetUserId();
        if (Context != null && !string.IsNullOrWhiteSpace(userId))
        {
            await _messageSession.Publish(new ClientDisconnectedEvent
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId, 
                Host = GetLocalHost()
            });
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetRecentChat(string chatId)
    {
        if (string.IsNullOrWhiteSpace(chatId))
        {
            return;
        }

        var userId = Context?.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            // should never happen
            throw new UnauthorizedAccessException();
        }

        var message = new RecentChatSelectedEvent
        {
            Id = Guid.NewGuid(),
            ChatId = chatId,
            SelectedBy = userId,
            ConnectionId = Context?.ConnectionId
        };

        await _messageSession.Publish(message);
    }

    public async Task GetMoreMessages(string chatId, long earlierThan)
    {
        const int pageSize = 25;

        if (string.IsNullOrWhiteSpace(chatId) || earlierThan < 0)
        {
            return;
        }

        var userId = Context?.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            // should never happen
            throw new UnauthorizedAccessException();
        }

        var earlierThanDateTime = DateTimeOffset.FromUnixTimeMilliseconds(earlierThan).UtcDateTime;
        var messages = _chatArchiveService.GetArchivedMessagesAsync(chatId, earlierThanDateTime, pageSize);
        var messageCount = 0;

        await foreach (var chatMessage in messages)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", new ChatMessageViewModel
            {
                User = chatMessage.From,
                Content = chatMessage.Content,
                ChatId = chatId,
                Direction = chatMessage.From == userId ? MessageDirection.Sent : MessageDirection.Received,
                Timestamp = (new DateTimeOffset(chatMessage.UtcTimestamp)).ToUnixTimeMilliseconds(),
                FromArchive = true
            });

            messageCount++;
        }

        await Clients.Caller.SendAsync("FinishedLoading", new { MoreData = messageCount == pageSize });
    }

    private string GetLocalHost()
    {
        var httpConnectionFeature = Context.Features.Get<IHttpConnectionFeature>();
        return $"{httpConnectionFeature?.LocalIpAddress}:{httpConnectionFeature?.LocalPort}";
    }
}