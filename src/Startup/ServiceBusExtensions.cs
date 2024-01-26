namespace MyUglyChat.Startup;

public static class ServiceBusExtensions
{
    public static IStartableEndpointWithExternallyManagedContainer RegisterServiceBus(this IServiceCollection services,
        string messageBusConnectionString)
    {
        var endpointConfiguration = new EndpointConfiguration("Marco.Chat");
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();

        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.ConnectionString(messageBusConnectionString);
        transport.UseConventionalRoutingTopology(QueueType.Quorum);

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.EnableInstallers();

        var startableEndpoint = EndpointWithExternallyManagedContainer.Create(endpointConfiguration, services);

        services.AddSingleton(p => startableEndpoint.MessageSession.Value);

        return startableEndpoint;
    }
}
