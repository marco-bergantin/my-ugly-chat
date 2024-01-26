namespace MyUglyChat.Startup;

public static class ServiceBusInstaller
{
    private const string EndpointName = "my-ugly-chat-endpoint";
    private const string ErrorQueueName = "error";

    public static IStartableEndpointWithExternallyManagedContainer InstallServiceBus(this IServiceCollection services, IConfigurationRoot config)
    {
        var rabbitMqConnectionString = config["ConnectionStrings:RabbitMq"];
        if (string.IsNullOrWhiteSpace(rabbitMqConnectionString))
        {
            throw new ArgumentException(nameof(rabbitMqConnectionString));
        }

        var endpointConfiguration = new EndpointConfiguration(EndpointName);
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();

        var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
        transport.ConnectionString(rabbitMqConnectionString);
        transport.UseConventionalRoutingTopology(QueueType.Quorum);

        endpointConfiguration.SendFailedMessagesTo(ErrorQueueName);
        endpointConfiguration.EnableInstallers();

        var startableEndpoint = EndpointWithExternallyManagedContainer.Create(endpointConfiguration, services);

        services.AddSingleton(p => startableEndpoint.MessageSession.Value);

        return startableEndpoint;
    }
}
