using MyUglyChat.DAL;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace MyUglyChat.Services;

public class ChatArchiveService(IMongoDatabase database)
{
    private readonly IMongoCollection<Chat> _chatArchiveCollection = database.GetCollection<Chat>("chat-archive");

    public async IAsyncEnumerable<ChatMessage> GetArchivedMessagesAsync(string chatId, DateTime earlierThan, int pageSize)
    {
        var messages = await _chatArchiveCollection.Aggregate()
            .Match(c => c.ChatId == chatId)
            .Unwind(c => c.Messages)
            .Match(Builders<BsonDocument>.Filter.Lt("Messages.UtcTimestamp", earlierThan))
            .SortByDescending(b => b["Messages.UtcTimestamp"])
            .Limit(pageSize)
            .Project(b => b["Messages"])
            .ToListAsync();

        foreach (var message in messages) 
        {
            yield return BsonSerializer.Deserialize<ChatMessage>(message.AsBsonDocument);
        }
    }

    public async Task ArchiveMessageAsync(string chatId, ChatMessage message)
    {
        await _chatArchiveCollection.UpdateOneAsync(c => c.ChatId == chatId,
            Builders<Chat>.Update.AddToSet(m => m.Messages, message), // AddToSet guarantees uniqueness in the Messages array
            new UpdateOptions { IsUpsert = true });
    }
}
