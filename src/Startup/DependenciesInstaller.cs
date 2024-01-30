using MyUglyChat.Services;

namespace MyUglyChat.Startup;

public static class DependenciesInstaller
{
    public static void InstallDependencies(this IServiceCollection services)
    {
        services.AddSingleton<UserConnectionService>();
        services.AddSingleton<ContactsService>();
        services.AddSingleton<ChatArchiveService>();
        services.AddSingleton<RecentChatsService>();
        services.AddSingleton<MetricsService>();
    }
}
