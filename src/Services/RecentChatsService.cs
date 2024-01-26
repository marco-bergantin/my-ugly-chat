using Auth0Mvc.DAL;
using MongoDB.Driver;

namespace Auth0Mvc.Services;

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

        var updateStatement = Builders<Chat>.Update.PushEach(m => m.Messages, new[] { message },
                slice: RecentMessagesCount,
                sort: Builders<ChatMessage>.Sort.Descending(m => m.UtcTimestamp));

        _ = await _recentChatsCollection.UpdateOneAsync(c => c.ChatId == chatId, 
            updateStatement, 
            new UpdateOptions { IsUpsert = true });
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
