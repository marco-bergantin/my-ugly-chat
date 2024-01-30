using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MyUglyChat.Startup;

public static class OpenTelemetryInstaller
{
    public static void InstallOpenTelemetryTracing(this IHostApplicationBuilder builder, IConfigurationRoot config)
    {
        var otelExporterEndpoint = config["ConnectionStrings:OtelExporter"];
        if (string.IsNullOrWhiteSpace(otelExporterEndpoint))
        {
            throw new ArgumentException(nameof(otelExporterEndpoint));
        }

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithMetrics(metrics => 
                metrics.AddAspNetCoreInstrumentation()
                       .AddMeter("Microsoft.AspNetCore.Hosting")
                       .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                       .AddMeter("MyUglyChat"))
            .WithTracing(tracing =>
                tracing.AddAspNetCoreInstrumentation()
                       .AddConsoleExporter()
                       .AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(otelExporterEndpoint)));
    }
}
