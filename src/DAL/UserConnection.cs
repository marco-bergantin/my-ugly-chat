using MongoDB.Bson.Serialization.Attributes;

namespace Auth0Mvc.DAL;

public class UserConnection
{
    [BsonId]
    public required string UserId { get; init; }
    public required string[] ConnectionIds { get; init; }
}
