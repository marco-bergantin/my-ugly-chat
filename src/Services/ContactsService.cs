using MyUglyChat.DAL;
using MongoDB.Driver;

namespace MyUglyChat.Services;

public class ContactsService(IMongoDatabase database)
{
    private readonly IMongoCollection<ContactList> _contactListsCollection = database.GetCollection<ContactList>("contact-lists");

    public async Task AddContactToListAsync(string ownerId, Contact newContact)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
        {
            throw new ArgumentNullException(nameof(ownerId));
        }

        if (newContact is null || string.IsNullOrWhiteSpace(newContact.UserId))
        {
            throw new ArgumentException(nameof(newContact));
        }

        _ = await _contactListsCollection.UpdateOneAsync(c => c.OwnerId == ownerId,
            Builders<ContactList>.Update.AddToSet(p => p.Contacts, newContact),
            new UpdateOptions { IsUpsert = true });
    }

    public async Task<ContactList> GetContactsAsync(string ownerId)
    {
        var result = await _contactListsCollection.FindAsync(c => c.OwnerId == ownerId);
        return result.SingleOrDefault();
    }

    public async Task UpdateLatestMessageTimestampAsync(string ownerId, string contactId, DateTime timestamp)
    {
        var filter = Builders<ContactList>.Filter.Eq(c => c.OwnerId, ownerId)
            & Builders<ContactList>.Filter.ElemMatch(c => c.Contacts, Builders<Contact>.Filter.Eq(c => c.UserId, contactId));

        var update = Builders<ContactList>.Update.Set("Contacts.$.TimestampLatestMessage", timestamp);

        _ = await _contactListsCollection.UpdateOneAsync(filter, update);
    }
}
