using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.FundingRuleValidation.Jobs.Core.Configuration;

[ExcludeFromCodeCoverage]
public static class HostBuilderExtensions
{
    public static FunctionsApplicationBuilder ConfigureFundingRuleValidationApp(this FunctionsApplicationBuilder builder)
    {
        builder
            .ConfigureFunctionsWebApplication();
            
        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables()
            .AddJsonFile("local.settings.json", optional: true);
            
        var environmentName = builder.Configuration.GetValue<string>("Values:EnvironmentName") ?? builder.Configuration.GetValue<string>("EnvironmentName");
        if (!environmentName!.Equals("DEV", StringComparison.CurrentCultureIgnoreCase))
        {
            builder.Configuration.AddAzureTableStorage(options =>
            {
                options.ConfigurationNameIncludesVersionNumber = true;
                options.PreFixConfigurationKeys = false;
#if DEBUG
                options.ConfigurationKeys = builder.Configuration.GetValue<string>("Values:ConfigNames")!.Split(",");
                options.StorageConnectionString = builder.Configuration.GetValue<string>("Values:ConfigurationStorageConnectionString");
                options.EnvironmentName = builder.Configuration.GetValue<string>("Values:EnvironmentName");
#else
                    options.ConfigurationKeys = builder.Configuration.GetValue<string>("ConfigNames")!.Split(",");
                    options.StorageConnectionString = builder.Configuration.GetValue<string>("ConfigurationStorageConnectionString");
                    options.EnvironmentName = builder.Configuration.GetValue<string>("EnvironmentName");
#endif
            });
        }
        
        builder.Services.AddOpenTelemetryRegistration(builder.Configuration.GetValue<string>("APPLICATIONINSIGHTS_CONNECTION_STRING"));
        return builder;
    }
}