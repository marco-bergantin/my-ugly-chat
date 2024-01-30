using System.Diagnostics.Metrics;

namespace MyUglyChat.Services;

public class MetricsService
{
    private readonly Counter<int> _messagesSent;

    public MetricsService(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MyUglyChat");
        _messagesSent = meter.CreateCounter<int>("messages-sent", "messages");
    }

    public void AddMessageSent() => _messagesSent.Add(1);
}
