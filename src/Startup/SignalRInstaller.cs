namespace MyUglyChat.Startup;

public static class SignalRInstaller
{
    public static void InstallSignalR(this IServiceCollection services, IConfigurationRoot config)
    {
        var redisConnectionString = config["ConnectionStrings:Redis"];
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new ArgumentException(nameof(redisConnectionString));
        }

        services.AddSignalR().AddStackExchangeRedis(redisConnectionString,
            o => o.Configuration.AbortOnConnectFail = false);
    }
}
