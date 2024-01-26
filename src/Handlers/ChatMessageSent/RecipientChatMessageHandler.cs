﻿using Auth0Mvc.Hubs;
using Auth0Mvc.Messages;
using Auth0Mvc.Models;
using Auth0Mvc.Services;
using Microsoft.AspNetCore.SignalR;

namespace Auth0Mvc.Handlers.ChatMessageSent;

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

            await hubClient.SendAsync("ReceiveMessage", new ChatMessageViewModel
            {
                User = message.From,
                ChatId = message.ChatId,
                Content = message.Content,
                Direction = MessageDirection.Received,
                Timestamp = (new DateTimeOffset(message.UtcTimestamp)).ToUnixTimeMilliseconds()
            }, context.CancellationToken);
        }
    }
}