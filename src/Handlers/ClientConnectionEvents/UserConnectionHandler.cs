using Auth0Mvc.Messages;
using Auth0Mvc.Services;

namespace Auth0Mvc.Handlers.ClientConnectionEvents;

public class UserConnectionHandler(UserConnectionService userConnectionService) : IHandleMessages<ClientConnectedEvent>,
    IHandleMessages<ClientDisconnectedEvent>
{
    private readonly UserConnectionService _userConnectionService = userConnectionService;

    public async Task Handle(ClientConnectedEvent message, IMessageHandlerContext context)
    {
        await _userConnectionService.AddUserConnectionAsync(message.UserId, message.ConnectionId);
    }

    public async Task Handle(ClientDisconnectedEvent message, IMessageHandlerContext context)
    {
        await _userConnectionService.RemoveUserConnectionAsync(message.UserId, message.ConnectionId);
    }
}
