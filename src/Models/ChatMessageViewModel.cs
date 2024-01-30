using MyUglyChat.DAL;

namespace MyUglyChat.Models;

public class ChatMessageViewModel
{
    public required Guid MessageId { get; init; }
    public required string User { get; init; }
    public required string ChatId { get; init; }
    public required string Content { get; init; }
    public required MessageDirection Direction { get; init; }
    public required long Timestamp { get; init; }
    public bool FromArchive { get; init; } = false; // TODO: think of a better name

    public static ChatMessageViewModel FromChatMessage(ChatMessage chatMessage, string chatId, string userId) => new()
    {
        MessageId = chatMessage.Id,
        User = chatMessage.From,
        Content = chatMessage.Content,
        ChatId = chatId,
        Direction = chatMessage.From == userId ? MessageDirection.Sent : MessageDirection.Received,
        Timestamp = (new DateTimeOffset(chatMessage.UtcTimestamp)).ToUnixTimeMilliseconds(),
        FromArchive = true // not exactly from archive but in reverse chronological order (so they need to be added on top in the UI)
    };
}
