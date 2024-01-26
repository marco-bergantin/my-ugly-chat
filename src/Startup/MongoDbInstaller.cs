using MongoDB.Driver;

namespace MyUglyChat.Startup;

public static class MongoDbInstaller
{
    private const string DatabaseName = "my-ugly-chat-db";

    public static void InstallMongoDb(this IServiceCollection services, IConfigurationRoot config)
    {
        var mongoDbConnectionString = config["ConnectionStrings:MongoDB"];
        if (string.IsNullOrWhiteSpace(mongoDbConnectionString))
        {
            throw new ArgumentException(nameof(mongoDbConnectionString));
        }

        var mongoClient = new MongoClient(mongoDbConnectionString);
        var database = mongoClient.GetDatabase(DatabaseName);
        services.AddSingleton(database);
    }
}
