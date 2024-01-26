using Auth0Mvc.DAL;
using MongoDB.Driver;

namespace Auth0Mvc.Services;

public class UserConnectionService(IMongoDatabase database)
{
    private readonly IMongoCollection<UserConnection> _userConnectionCollection = database.GetCollection<UserConnection>("user-connections");

    public async Task AddUserConnectionAsync(string userId, string connectionId)
    {
        _ = await _userConnectionCollection.UpdateOneAsync(c => c.UserId == userId,
               Builders<UserConnection>.Update.AddToSet(c => c.ConnectionIds, connectionId),
               new UpdateOptions { IsUpsert = true });
    }

    public async Task<string[]> GetConnectionIdsAsync(string userId)
    {
        var recordCursor = await _userConnectionCollection.FindAsync(c => c.UserId == userId);
        var record = await recordCursor.SingleOrDefaultAsync();
        return record?.ConnectionIds ?? [];
    }

    public async Task RemoveUserConnectionAsync(string userId, string connectionId)
    {
        _ = await _userConnectionCollection.UpdateOneAsync(c => c.UserId == userId,
               Builders<UserConnection>.Update.Pull(c => c.ConnectionIds, connectionId));
    }
}
