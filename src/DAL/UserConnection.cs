using MongoDB.Bson.Serialization.Attributes;

namespace MyUglyChat.DAL;

public class UserConnection
{
    [BsonId]
    public required string UserId { get; init; }
    public required string[] ConnectionIds { get; init; }
}
