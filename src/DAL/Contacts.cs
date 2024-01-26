using MongoDB.Bson.Serialization.Attributes;

namespace Auth0Mvc.DAL;

public class ContactList
{
    [BsonId]
    public required string OwnerId { get; set; }
    public required Contact[] Contacts { get; set; }
}

public class Contact
{
    public required string DisplayName { get; set; }
    public required string UserId { get; set; }
    public DateTime TimestampLatestMessage { get; set; }
}
