using MyUglyChat.Helpers;
using MyUglyChat.Messages;
using MyUglyChat.Models;
using MyUglyChat.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace MyUglyChat.Hubs;

public class ChatHub(IMessageSession messageSession, 
    ContactsService contactsService,
    ChatArchiveService chatArchiveService,
    RecentChatsService recentChatsService) : Hub
{
    private readonly IMessageSession _messageSession = messageSession;
    private readonly ContactsService _contactsService = contactsService;
    private readonly ChatArchiveService _chatArchiveService = chatArchiveService;
    private readonly RecentChatsService _recentChatsService = recentChatsService;

    public async Task SendMessage(string chatId, string to, string message)
    {
        var senderUserId = ValidateAndGetUserId();

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

        // only publish the event here, sender already has message displayed
        // see sendMessage function in chat.js
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
                Host = GetLocalHostAddress()
            });           
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
                Host = GetLocalHostAddress()
            });
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetRecentContacts()
    {
        var userId = ValidateAndGetUserId();
        var contactList = await _contactsService.GetContactsAsync(userId);
        var contactsViewModel = ContactsViewModel.FromContactList(contactList, userId);

        await Clients.Client(Context.ConnectionId).SendAsync("RefreshRecentContacts", contactsViewModel);
    }

    public async Task GetRecentChat(string chatId)
    {
        if (string.IsNullOrWhiteSpace(chatId))
        {
            throw new ArgumentException(nameof(chatId));
        }

        var userId = ValidateAndGetUserId();

        var recipientConnectionId = Context.ConnectionId;
        if (string.IsNullOrWhiteSpace(recipientConnectionId))
        {
            // should never happen, as it's handled by SignalR
            throw new InvalidOperationException(nameof(Context.ConnectionId));
        }

        var hubClient = Clients.Client(recipientConnectionId);
        if (hubClient is null)
        {
            // we think we have this connection for this user, but it actually fell from the hub
            return;
        }

        var recentChat = await _recentChatsService.GetRecentChatAsync(chatId);
        if (recentChat is null)
        {
            // no recent chat found, log error
            return; 
        }

        foreach (var chatMessage in recentChat.Messages)
        {
            await hubClient.SendAsync("ReceiveMessage", ChatMessageViewModel.FromChatMessage(chatMessage, chatId, userId));
        }
    }

    public async Task GetMoreMessages(string chatId, long earlierThan)
    {
        const int pageSize = 25;

        if (string.IsNullOrWhiteSpace(chatId) || earlierThan < 0)
        {
            return;
        }

        var userId = ValidateAndGetUserId();

        var earlierThanDateTime = DateTimeOffset.FromUnixTimeMilliseconds(earlierThan).UtcDateTime;
        var messages = _chatArchiveService.GetArchivedMessagesAsync(chatId, earlierThanDateTime, pageSize);
        var messageCount = 0;

        await foreach (var chatMessage in messages)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", ChatMessageViewModel.FromChatMessage(chatMessage, chatId, userId));
            messageCount++;
        }

        await Clients.Caller.SendAsync("FinishedLoading", new { MoreData = messageCount == pageSize });
    }

    private string ValidateAndGetUserId()
    {
        if (Context is null)
        {
            // should never happen, as it's handled by SignalR
            throw new InvalidOperationException(nameof(Context));
        }

        var userId = Context.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            // should never happen, bc the user needs to be authenticated
            throw new UnauthorizedAccessException();
        }

        return userId;
    }

    private string GetLocalHostAddress()
    {
        var httpConnectionFeature = Context.Features.Get<IHttpConnectionFeature>();
        return $"{httpConnectionFeature?.LocalIpAddress}:{httpConnectionFeature?.LocalPort}";
    }
}