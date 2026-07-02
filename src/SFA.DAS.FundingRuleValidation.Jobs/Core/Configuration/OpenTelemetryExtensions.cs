using System.Diagnostics.CodeAnalysis;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.FundingRuleValidation.Jobs.Core.Configuration;

[ExcludeFromCodeCoverage]
public static class OpenTelemetryExtensions
{
    public static void AddOpenTelemetryRegistration(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }
        
        services.AddOpenTelemetry().UseAzureMonitorExporter(options => { options.ConnectionString = connectionString; });
    }
}