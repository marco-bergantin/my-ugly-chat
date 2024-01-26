using MongoDB.Bson.Serialization.Attributes;

namespace Auth0Mvc.DAL;

public class Chat
{
    [BsonId] // {userId1}_{userId2} (in alphabetical order)
    public required string ChatId { get; set; } 
    public ChatMessage[] Messages { get; set; } = [];
}

public class ChatMessage
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public required DateTime UtcTimestamp { get; set; }

    // this is sufficient (no need for To property) since the chatId is per user pair (should be good enough later for group chats as well)
    public required string From { get; set; }
}
