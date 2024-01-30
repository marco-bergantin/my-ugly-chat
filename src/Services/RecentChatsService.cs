using MyUglyChat.DAL;
using MongoDB.Driver;

namespace MyUglyChat.Services;

public class RecentChatsService(IMongoDatabase database)
{
    private const int RecentMessagesCount = 25;

    private readonly IMongoCollection<Chat> _recentChatsCollection = database.GetCollection<Chat>("recent-chats");

    public async Task<Chat> GetRecentChatAsync(string chatId)
    {
        var recentChatCursor = await _recentChatsCollection.FindAsync(c => c.ChatId == chatId);
        return await recentChatCursor.SingleOrDefaultAsync();
    }

    public async Task StoreRecentChatMessageAsync(string userIdTo, string userIdFrom, ChatMessage message)
    {
        var chatId = GetChatId(userIdTo, userIdFrom);

        var filter = Builders<Chat>.Filter.Eq(c => c.ChatId, chatId)
           & Builders<Chat>.Filter.Not(Builders<Chat>.Filter.ElemMatch(c => c.Messages, Builders<ChatMessage>.Filter.Eq(m => m.Id, message.Id)));

        var updateStatement = Builders<Chat>.Update.PushEach(m => m.Messages, new[] { message },
                slice: RecentMessagesCount,
                sort: Builders<ChatMessage>.Sort.Descending(m => m.UtcTimestamp));

        try
        {
            _ = await _recentChatsCollection.UpdateOneAsync(filter, updateStatement, new UpdateOptions { IsUpsert = true });
        }
        catch (MongoWriteException mwe)
        {
            if (mwe.WriteError.Category == ServerErrorCategory.DuplicateKey)
            {
                // log error, but do nothing else
                // filter will match on chatId, then check the contents of the Messages array to verify the message hasn't been inserted yet
                // if it has, the filter won't match, and since the operation is an upsert, the update will try to insert the record anew
                // which will fail, since there's already a document with key {chatId}
                // there's probably a better way to ensure uniqueness in an embedded array (and idempotency of this operation),
                // but not with indexing, see https://www.mongodb.com/community/forums/t/unique-key-on-array-fields-in-a-single-document/3453/4
                return;
            }

            throw;
        }
    }

    public static string GetChatId(string userIdTo, string userIdFrom)
    {
        if (string.IsNullOrWhiteSpace(userIdTo))
        {
            throw new ArgumentException(nameof(userIdTo));
        }

        if (string.IsNullOrWhiteSpace(userIdFrom))
        {
            throw new ArgumentException(nameof(userIdFrom));
        }

        return userIdTo.CompareTo(userIdFrom) >= 0
            ? $"{userIdFrom}_{userIdTo}"
            : $"{userIdTo}_{userIdFrom}";
    }
}
