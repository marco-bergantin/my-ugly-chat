namespace Auth0Mvc.Models;

public class ChatMessageViewModel
{
    public required string User { get; init; }
    public required string ChatId { get; init; }
    public required string Content { get; init; }
    public required MessageDirection Direction { get; init; }
    public required long Timestamp { get; init; }
    public bool FromArchive { get; init; } = false; // TODO: think of a better name
}
